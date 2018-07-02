using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.Websock;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using WebSocketSharp;

namespace NetMud.Gossip
{
    public class GossipServer
    {
        public int PortNumber { get; private set; }

        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat { get { return "GossipWebSocket.{0}"; } }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy { get { return ConnectionType.Websocket; } }

        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        private const string EncapsulationElement = "";

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
        private const string BumperElement = "/n/r";

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        public void Launch(int portNumber)
        {
            ConnectedClients = new List<IDescriptor>();

            var service = new TcpListener(IPAddress.Parse("127.0.0.1"), portNumber);

            PortNumber = portNumber;

            LiveCache.Add(service, string.Format(cacheKeyFormat, portNumber));

            service.Start(128);

            service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);

            try
            {
                //Connect to the gossip service
                var talkingSocket = new WebSocket("wss://gossip.haus/socket");

                talkingSocket.Log.Level = LogLevel.Trace;
                talkingSocket.Log.Output = (data, eventing) => LoggingUtility.Log(data.Message, LogChannels.GossipServer, true);

                talkingSocket.OnMessage += (sender, e) =>
                    Console.WriteLine("Gossip says: " + e.Data);

                talkingSocket.Connect();

                if(talkingSocket.IsAlive)
                {
                    talkingSocket.Send("BALUS");
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.GossipServer);
            }
        }

        public IList<IDescriptor> ConnectedClients { get; private set; }

        public bool Broadcast(string message)
        {
            var service = GetActiveService<TcpListener>();

            if (service == null)
                return false;

            return service.Server.Send(Encoding.ASCII.GetBytes(message)) > 0;
        }

        public void Shutdown()
        {
            var service = GetActiveService<TcpListener>();

            if (service != null)
                service.Stop();
        }

        public T GetActiveService<T>()
        {
            try
            {
                return LiveCache.Get<T>(string.Format(cacheKeyFormat, PortNumber));
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
            }

            return default(T);
        }

        private void OnAccept(IAsyncResult result)
        {
            var service = (TcpListener)result.AsyncState;

            try
            {
                var newDescriptor = new Descriptor(service.EndAcceptTcpClient(result));

                ConnectedClients.Add(newDescriptor);

                newDescriptor.Open();
            }
            catch
            {
                //TODO: Logging
            }

            service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);
        }
    }

}
