using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.Gossip.Messaging;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace NetMud.Gossip
{
    public class GossipClient : IGossipClient
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string CacheKey => "GossipWebClient";

        private WebSocket MyClient { get; set; }

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        public void Launch()
        {
            try
            {
                //Connect to the gossip service
                GetNewSocket();

                MyClient.Connect();

                LiveCache.Add(this, CacheKey);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.GossipServer);
            }
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private void OnClose(object sender, EventArgs e)
        {
            LoggingUtility.Log("Gossip Server Connection Terminated.", LogChannels.GossipServer);

            MyClient = null;

            ReconnectLoop(Launch);
        }

        /// <summary>
        /// Handles the wait loop for accepting input from the socket
        /// </summary>
        /// <param name="worker">the function that actually takes in a full message from the socker</param>
        private async void ReconnectLoop(Action worker)
        {
            if (MyClient != null && MyClient.IsAlive)
                return;

            try
            {
                if (MyClient == null)
                    GetNewSocket();

                if (MyClient.Ping())
                {
                    worker.Invoke();
                    return;
                }

                await Task.Delay(10000);

                ReconnectLoop(worker);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.GossipServer);
            }
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private void OnOpen(object sender, EventArgs e)
        {
            var authenticateBlock = new Authentication();

            var auth = new TransportMessage()
            {
                Event = authenticateBlock.Type,
                Payload = authenticateBlock
            };

            MyClient.Send(Serialize(auth));
        }

        private void OnMessage(object sender, MessageEventArgs e)
        {
            if (e.IsPing)
                return; //pong maybe?
            else
            {
                LoggingUtility.Log(e.Data, LogChannels.GossipServer, true);
                var newReply = DeSerialize(e.Data);

                switch (newReply.Event)
                {
                    case "heartbeat":
                        var whoList = LiveCache.GetAll<IPlayer>().Where(player => player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                                                && player.Descriptor != null);

                        var whoBlock = new HeartbeatResponse()
                        {
                            Players = whoList.Select(player => player.AccountHandle).ToArray()
                        };

                        var response = new TransportMessage()
                        {
                            Event = whoBlock.Type,
                            Payload = whoBlock
                        };

                        MyClient.Send(Serialize(response));
                        break;
                    case "messages/direct":
                        var validPlayer = LiveCache.GetAll<IPlayer>().FirstOrDefault(player => player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                                                && player.AccountHandle.Equals(newReply.Payload.name)
                                                                && player.Descriptor != null);

                        if (validPlayer != null)
                            validPlayer.Descriptor.SendWrapper(newReply.Payload.MessageBody);
                        break;
                    case "messages/broadcast":
                        var messageText = newReply.Payload.message.Value;
                        var messageSender = newReply.Payload.name.Value;
                        var source = newReply.Payload.game.Value;
                        var channel = newReply.Payload.channel.Value;

                        if (!string.IsNullOrWhiteSpace(messageText))
                        {
                            var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber);
                            foreach (var player in validPlayers)
                                player.WriteTo(new string[] { string.Format("{0}@{1} {3}s, '{2}'", messageSender, source, messageText, channel) });
                        }
                        break;
                    default:
                        //do nothing
                        break;
                }
            }
        }

        private void GetNewSocket()
        {
            MyClient = new WebSocket("wss://gossip.haus/socket");

            MyClient.Log.Level = LogLevel.Error;
            MyClient.Log.Output = (data, eventing) => LoggingUtility.Log(data.Message, LogChannels.GossipServer, true);

            MyClient.OnMessage += (sender, e) => OnMessage(sender, e);

            MyClient.OnOpen += (sender, e) => OnOpen(sender, e);

            MyClient.OnClose += (sender, e) => OnClose(sender, e);
        }

        public void SendMessage(string userName, string messageBody, string channel = "gossip")
        {
            var messageBlock = new NewMessage()
            {
                ChannelName = channel,
                MessageBody = messageBody,
                Username = userName
            };

            var message = new TransportMessage()
            {
                Event = messageBlock.Type,
                Payload = messageBlock
            };

            MyClient.Send(Serialize(message));
        }

        public void Shutdown()
        {
            var service = GetActiveService();

            if (service != null)
                service.Close();
        }

        public WebSocket GetActiveService()
        {
            try
            {
                return LiveCache.Get<WebSocket>(CacheKey);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }

            return null;
        }

        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        private string Serialize(TransportMessage message)
        {
            var serializer = SerializationUtility.GetSerializer();

            serializer.TypeNameHandling = TypeNameHandling.None;

            var sb = new StringBuilder();
            var writer = new StringWriter(sb);

            serializer.Serialize(writer, message);

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize a json string into this entity
        /// </summary>
        /// <param name="jsonData">string to deserialize</param>
        /// <returns>the entity</returns>
        private TransportMessage DeSerialize(string jsonData)
        {
            var serializer = SerializationUtility.GetSerializer();

            var reader = new StringReader(jsonData);

            return serializer.Deserialize(reader, typeof(TransportMessage)) as TransportMessage;
        }
    }

}
