using NetMud.Communication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace NetMud.Websock.TestServer
{
    public class Server : WebSocketServer, IServer
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat => "LiveWebSocket.{0}";

        public IList<IDescriptor> ConnectedClients { get; set; }

        public int PortNumber => Port;

        public Server(int port, bool secure) : base(port, secure)
        {
            Log.Output = (data, eventing) => LoggingUtility.Log(data.Message, LogChannels.SocketCommunication, true);
            Log.Level = LogLevel.Trace;
            AddWebSocketService<Descriptor>("/");
        }

        public bool Broadcast(string message)
        {
            foreach(var client in ConnectedClients)
            {
                client.SendWrapper(message);
            }

            return true;
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

        public void Shutdown()
        {
            Stop(CloseStatusCode.Normal, "Shutdown by request.");
        }

        public void Launch(int portNumber)
        {
            LiveCache.Add(this, string.Format(cacheKeyFormat, Port));
            Start();

            if (IsListening)
            {
                LoggingUtility.Log(string.Format("{1} websocket started on port {0}.", Port, IsSecure ? "Secure" : "Insecure"), LogChannels.SocketCommunication, true);
            }
        }
    }
}
