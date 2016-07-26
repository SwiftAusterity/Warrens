using System;
using System.Net.Sockets;
using System.Net;
using NetMud.Communication;
using System.Threading.Tasks;
using System.Text;
using System.Collections.Generic;
using NetMud.DataAccess;

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
            ConnectedClients = new List<IDescriptor>();

            var service = new TcpListener(IPAddress.Parse("127.0.0.1"), portNumber);

            PortNumber = portNumber;

            LiveCache.Add(service, String.Format(cacheKeyFormat, portNumber));

            service.Start(128);

            service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);
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
                return LiveCache.Get<T>(String.Format(cacheKeyFormat, PortNumber));
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return default(T);
        }

        private void OnAccept(IAsyncResult result)
        {
            var service = (TcpListener)result.AsyncState;

            try
            {
                //if (!service.Pending())
                //    return;

                var newDescriptor = new Descriptor(service.EndAcceptTcpClient(result));

                ConnectedClients.Add(newDescriptor);
            }
            catch
            {
                //TODO: Logging
            }

            service.BeginAcceptTcpClient(new AsyncCallback(OnAccept), service);
        }
    }
}