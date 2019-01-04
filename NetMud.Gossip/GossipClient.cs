using NetMud.Gossip.Messaging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace NetMud.Gossip
{
    public class GossipClient
    {
        private WebSocket MyClient { get; set; }

        private IConfig ConfigSettings { get; }

        private Action<Exception> ExceptionLogger { get; }

        private Action<string> ActivityLogger { get; }

        private Func<Member[]> UserList { get; }

        public GossipClient(IConfig config, Action<Exception> exLogger, Action<string> activityLogger, Func<Member[]> getUserList)
        {
            ConfigSettings = config;
            ExceptionLogger = exLogger;
            ActivityLogger = activityLogger;
            UserList = getUserList;
        }

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        public async void Launch()
        {
            if (string.IsNullOrWhiteSpace(ConfigSettings.ClientId) || string.IsNullOrWhiteSpace(ConfigSettings.ClientSecret))
            {
                DoLog("No credentials to connect to Gossip with, ending.");
                return;
            }

            try
            {
                //Connect to the gossip service
                GetNewSocket();

                MyClient.Connect();

                if (MyClient.ReadyState != WebSocketState.Open)
                {
                    throw new TimeoutException("Gossip Server unresponsive on open. Starting reconnect loop.");
                }

                await Task.Delay(1);
            }
            catch (TimeoutException tex)
            {
                DoLog(tex);
                ReconnectLoop(60);
            }
            catch(System.Net.Sockets.SocketException sex)
            {
                DoLog(sex); //Dont retry on this
            }
            catch (Exception ex)
            {
                DoLog(ex);
                ReconnectLoop(10);
            }
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private void OnClose(object sender, EventArgs e)
        {
            DoLog(new Exception("Gossip Server Connection Terminated."));

            MyClient = null;

            ReconnectLoop();
        }

        /// <summary>
        /// Handles the wait loop for accepting input from the socket
        /// </summary>
        /// <param name="worker">the function that actually takes in a full message from the socker</param>
        private async void ReconnectLoop(double suspendMultiplier = 1)
        {
            if (MyClient != null && MyClient.IsAlive || suspendMultiplier > ConfigSettings.SuspendMultiplierMaximum)
                return;

            DoLog("Gossip Server Reconnect Loop Pulse x" + suspendMultiplier.ToString());

            try
            {
                if (MyClient == null)
                    GetNewSocket();

                MyClient.Connect();

                if (MyClient.ReadyState == WebSocketState.Open)
                {
                    DoLog("Gossip Server Reconnect Loop Successful.");
                    return;
                }

                await Task.Delay(Convert.ToInt32(10000 * suspendMultiplier));

                ReconnectLoop(suspendMultiplier * ConfigSettings.SuspendMultiplier);
            }
            catch (Exception ex)
            {
                DoLog(ex);
            }
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private void OnOpen(object sender, EventArgs e)
        {
            Authentication authenticateBlock = new Authentication(ConfigSettings);

            TransportMessage auth = new TransportMessage()
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

            TransportMessage newReply = DeSerialize(e.Data);

            switch (newReply.Event)
            {
                case "restart":
                    dynamic downtime = newReply.Payload.downtime;

                    MyClient.Close(CloseStatusCode.Normal);

                    Task.Run(() => Task.Delay(Convert.ToInt32(1000 * downtime)))
                        .ContinueWith((t) => ReconnectLoop());      

                    ReconnectLoop();
                    break;
                case "heartbeat":
                    HeartbeatResponse whoBlock = new HeartbeatResponse()
                    {
                        Players = UserList().Select(player => player.Name).ToArray()
                    };

                    TransportMessage response = new TransportMessage()
                    {
                        Event = whoBlock.Type,
                        Payload = whoBlock
                    };

                    MyClient.Send(Serialize(response));
                    break;
                case "tells/receive":
                    dynamic myName = newReply.Payload.player.Value;
                    dynamic theirName = newReply.Payload.from.Value;
                    dynamic theirGame = newReply.Payload.game.Value;
                    dynamic messageBody = newReply.Payload.message.Value;
                    dynamic fullName = string.Format("{0}@{1}", theirGame, theirName);

                    Member validPlayer = UserList().FirstOrDefault(user => user.Name.Equals(myName, StringComparison.InvariantCultureIgnoreCase));

                    if (validPlayer != null)
                        validPlayer.WriteTo(string.Format("{0} gossip-tells you, '{1}'", fullName, messageBody));
                    break;
                case "channels/broadcast":
                    string messageText = newReply.Payload.message.Value;
                    string messageSender = newReply.Payload.name.Value;
                    string source = newReply.Payload.game.Value;
                    string channel = newReply.Payload.channel.Value;

                    if (!string.IsNullOrWhiteSpace(messageText))
                    {
                        foreach (Member user in UserList().Where(usr => !usr.BlockedMembers.Contains(messageSender)))
                            user.WriteTo(string.Format("{0}@{1} {3}s, '{2}'", messageSender, source, messageText, channel));
                    }
                    break;
                case "players/sign-in":
                    if (newReply.ReferenceID == null)
                    {
                        //This is a request from the server
                        string fullSignInName = string.Format("{0}@{1}", newReply.Payload.game.Value, newReply.Payload.name.Value);

                        foreach (Member user in UserList().Where(usr => usr.Friends.Contains(fullSignInName)))
                            user.WriteTo(string.Format("{0} has logged into GOSSIP.", fullSignInName));
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
                        string fullSignoutName = string.Format("{0}@{1}", newReply.Payload.game.Value, newReply.Payload.name.Value);

                        foreach (Member user in UserList().Where(usr => usr.Friends.Contains(fullSignoutName)))
                            user.WriteTo(string.Format("{0} has logged out of GOSSIP.", fullSignoutName));
                    }
                    else
                    {
                        //This is a response to our request
                    }
                    break;
                case "channels/subscribe":
                case "channels/unsubscribe":
                case "channels/send":
                case "tells/send":
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
            MyClient.Log.Output = (data, eventing) => DoLog(data.Message);

            MyClient.OnMessage += (sender, e) => OnMessage(sender, e);

            MyClient.OnOpen += (sender, e) => OnOpen(sender, e);

            MyClient.OnClose += (sender, e) => OnClose(sender, e);

            MyClient.WaitTime = new TimeSpan(0, 0, 10);
        }

        public void SendMessage(string userName, string messageBody, string channel = "gossip")
        {
            NewMessage messageBlock = new NewMessage()
            {
                ChannelName = channel,
                MessageBody = messageBody,
                Username = userName
            };

            TransportMessage message = new TransportMessage()
            {
                Event = messageBlock.Type,
                Payload = messageBlock
            };

            MyClient.Send(Serialize(message));
        }

        public void SendDirectMessage(string userName, string targetGame, string targetPlayer, string messageBody)
        {
            NewDirectMessage messageBlock = new NewDirectMessage()
            {
                Gamename = targetGame,
                Target = targetPlayer,
                MessageBody = messageBody,
                Username = userName
            };

            TransportMessage message = new TransportMessage()
            {
                Event = messageBlock.Type,
                Payload = messageBlock
            };

            MyClient.Send(Serialize(message));
        }


        public void SendNotification(string userName, Notifications type)
        {
            TransportMessage message = new TransportMessage();

            switch (type)
            {
                case Notifications.LogIn:
                case Notifications.EnterGame:
                    SignIn loginNotify = new SignIn()
                    {
                        PlayerName = userName
                    };

                    message.Event = loginNotify.Type;
                    message.Payload = loginNotify;
                    break;
                case Notifications.LogOff:
                case Notifications.LeaveGame:
                    SignOut logoutNotify = new SignOut()
                    {
                        PlayerName = userName
                    };

                    message.Event = logoutNotify.Type;
                    message.Payload = logoutNotify;
                    break;
            }

            MyClient.Send(Serialize(message));
        }

        public void Shutdown()
        {
            if (MyClient != null)
                MyClient.Close();
        }

        /// <summary>
        /// Serialize this live entity to a json string
        /// </summary>
        /// <returns>json string</returns>
        private string Serialize(TransportMessage message)
        {
            JsonSerializer serializer = SerializationUtility.GetSerializer();

            serializer.TypeNameHandling = TypeNameHandling.None;

            StringBuilder sb = new StringBuilder();
            StringWriter writer = new StringWriter(sb);

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
            try
            {
                JsonSerializer serializer = SerializationUtility.GetSerializer();

                StringReader reader = new StringReader(jsonData);

                return serializer.Deserialize(reader, typeof(TransportMessage)) as TransportMessage;
            }
            catch (Exception ex)
            {
                DoLog(ex);
            }

            return new TransportMessage() { Status = "failure" };
        }

        private void DoLog(string toLog)
        {
            if (ActivityLogger == null)
                return;

            ActivityLogger(toLog);
        }

        private void DoLog(Exception ex)
        {
            if (ExceptionLogger == null)
                return;

            ExceptionLogger(ex);
        }
    }

}
