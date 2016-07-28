using System;
using System.Collections.Generic;
using NetMud.Communication;
using System.Net;
using NetMud.Authentication;
using System.Threading.Tasks;

namespace NetMud.Telnet
{
    public class Client : Channel, IDescriptor
    {
        public IPEndPoint remoteEndPoint { get; private set; }
        public DateTime connectedAt { get; private set; }
        public EClientState clientState { get; set; }
        public string commandIssued { get; set; }

        public string CacheKey 
        { 
            get
            {
                return String.Format(cacheKeyFormat, remoteEndPoint.Address);
            }
        }

        public Client(IPEndPoint _remoteEndPoint, DateTime _connectedAt, EClientState _clientState)
        {
            remoteEndPoint = _remoteEndPoint;
            connectedAt = _connectedAt;
            clientState = _clientState;
            commandIssued = String.Empty;
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Open()
        {
            throw new NotImplementedException();
        }

        public bool SendWrapper(IEnumerable<string> strings)
        {
            throw new NotImplementedException();
        }

        public bool SendWrapper(string str)
        {
            throw new NotImplementedException();
        }

        public void Disconnect(string finalMessage)
        {
            throw new NotImplementedException();
        }
    }
}
