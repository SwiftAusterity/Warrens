using NetMud.Communication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NetMud.Websock
{
    public class Server : Channel, IServer
    {
        public int PortNumber { get; private set; }

        /// <summary>
        /// Registers this for a service on a port
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        public void Launch(int portNumber)
        {
            try
            {
                ConnectedClients = new List<IDescriptor>();

                IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint localEndPoint = new IPEndPoint(ipAddress, portNumber);

                var service = new TcpListener(localEndPoint);
                PortNumber = portNumber;

                LoggingUtility.Log(string.Format("Websocket Connecting on: {0}, {1}", ipHostInfo.HostName, ipAddress.ToString()), LogChannels.SocketCommunication);

                LiveCache.Add(service, string.Format(cacheKeyFormat, portNumber));

                service.Start(128);

                service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SocketCommunication);
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
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SocketCommunication);
            }

            service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);
        }
    }
}