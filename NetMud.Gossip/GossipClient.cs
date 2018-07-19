using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
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
        public async void Launch()
        {
            try
            {
                //Connect to the gossip service
                GetNewSocket();

                MyClient.Connect();

                if (MyClient.ReadyState == WebSocketState.Open)
                {
                    LiveCache.Add(this, CacheKey);
                }
                else
                {
                    throw new TimeoutException("Gossip Server unresponsive on open. Starting reconnect loop.");
                }

                await Task.Delay(1);
            }
            catch (TimeoutException tex)
            {
                LoggingUtility.LogError(tex, LogChannels.GossipServer);
                ReconnectLoop(60);
            }
            catch(System.Net.Sockets.SocketException sex)
            {
                LoggingUtility.LogError(sex, LogChannels.GossipServer); //Dont retry on this
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.GossipServer);
                ReconnectLoop(1);
            }
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private void OnClose(object sender, EventArgs e)
        {
            LoggingUtility.Log("Gossip Server Connection Terminated.", LogChannels.GossipServer);

            MyClient = null;

            ReconnectLoop();
        }

        /// <summary>
        /// Handles the wait loop for accepting input from the socket
        /// </summary>
        /// <param name="worker">the function that actually takes in a full message from the socker</param>
        private async void ReconnectLoop(double suspendMultiplier = 1)
        {
            if (MyClient != null && MyClient.IsAlive || suspendMultiplier > 500)
                return;

            LoggingUtility.Log("Gossip Server Reconnect Loop Pulse x" + suspendMultiplier.ToString(), LogChannels.GossipServer);

            try
            {
                if (MyClient == null)
                    GetNewSocket();

                MyClient.Connect();

                if (MyClient.ReadyState == WebSocketState.Open)
                {
                    LoggingUtility.Log("Gossip Server Reconnect Loop Successful.", LogChannels.GossipServer);
                    return;
                }

                await Task.Delay(Convert.ToInt32(10000 * suspendMultiplier));

                ReconnectLoop(suspendMultiplier * 1.15);
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
            if(e == null || e.Data == null)
                return;

            var newReply = DeSerialize(e.Data);

            switch (newReply.Event)
            {
                case "heartbeat":
                    var whoList = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber);

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
                    var myName = newReply.Payload.playerName.Value;
                    var theirName = newReply.Payload.name.Value;
                    var theirGame = newReply.Payload.game.Value;
                    var messageBody = newReply.Payload.message.Value;
                    var fullName = string.Format("{0}@{1}", newReply.Payload.game.Value, newReply.Payload.name.Value);

                    var validPlayer = LiveCache.GetAll<IPlayer>().FirstOrDefault(player => player.Descriptor != null
                                                            && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                                            && player.AccountHandle.Equals(myName)
                                                            && !player.DataTemplate<ICharacter>().Account.Config.IsBlocking(fullName, true));

                    if (validPlayer != null)
                        validPlayer.WriteTo(new string[] { string.Format("{0} gossip-tells you, '{1}'", fullName, messageBody) });
                    break;
                case "channels/broadcast":
                    var messageText = newReply.Payload.message.Value;
                    var messageSender = newReply.Payload.name.Value;
                    var source = newReply.Payload.game.Value;
                    var channel = newReply.Payload.channel.Value;

                    if (!string.IsNullOrWhiteSpace(messageText))
                    {
                        var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null
                                    && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                    && !player.DataTemplate<ICharacter>().Account.Config.IsBlocking(messageSender, true));

                        foreach (var player in validPlayers)
                            player.WriteTo(new string[] { string.Format("{0}@{1} {3}s, '{2}'", messageSender, source, messageText, channel) });
                    }
                    break;
                case "players/sign-in":
                    if (newReply.ReferenceID == null)
                    {
                        //This is a request from the server
                        var fullSignInName = string.Format("{0}@{1}", newReply.Payload.game.Value, newReply.Payload.name.Value);

                        var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null
                                                                    && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                                                    && player.DataTemplate<ICharacter>().Account.Config.WantsNotification(fullSignInName, true, AcquaintenceNotifications.LogIn));
                        foreach (var player in validPlayers)
                            player.WriteTo(new string[] { string.Format("{0} has logged into GOSSIP.", fullSignInName) });
                    }
                    else
                    {
                        //This is a response to our request
                    }
                    break;
                case "players/sign-out":
                    if (newReply.ReferenceID == null)
                    {
                        //This is a request from the server
                        var fullSignoutName = string.Format("{0}@{1}", newReply.Payload.game.Value, newReply.Payload.name.Value);

                        var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null
                                                                    && player.DataTemplate<ICharacter>().Account.Config.GossipSubscriber
                                                                    && player.DataTemplate<ICharacter>().Account.Config.WantsNotification(fullSignoutName, true, AcquaintenceNotifications.LogOff));
                        foreach (var player in validPlayers)
                            player.WriteTo(new string[] { string.Format("{0} has logged out of GOSSIP.", fullSignoutName) });
                    }
                    else
                    {
                        //This is a response to our request
                    }
                    break;

                case "channels/subscribe":
                case "channels/unsubscribe":
                case "channels/send":
                case "authenticate":
                    //These are the "request-response"
                    if (newReply.Status.Equals("failure"))
                    {
                        //Do something?
                    }

                    break;
                default:
                    //do nothing
                    break;
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

        public void SendNotification(string userName, AcquaintenceNotifications type)
        {
            var message = new TransportMessage();

            switch (type)
            {
                case AcquaintenceNotifications.EnterGame:
                    var loginNotify = new SignIn()
                    {
                        PlayerName = userName
                    };

                    message.Event = loginNotify.Type;
                    message.Payload = loginNotify;
                    break;
                case AcquaintenceNotifications.LeaveGame:
                    var logoutNotify = new SignOut()
                    {
                        PlayerName = userName
                    };

                    message.Event = logoutNotify.Type;
                    message.Payload = logoutNotify;
                    break;
                case AcquaintenceNotifications.LogIn:
                case AcquaintenceNotifications.LogOff:
                    break; //Unused for now, when the "gossip client" comes in these can be used.
            }

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
