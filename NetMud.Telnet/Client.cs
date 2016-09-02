using System;
using System.Collections.Generic;
using System.Net;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;

namespace NetMud.Telnet
{
    public class Client : Channel, IDescriptor
    {
        public IPEndPoint remoteEndPoint { get; private set; }
        public DateTime connectedAt { get; private set; }
        public EClientState clientState { get; set; }
        public string commandIssued { get; set; }

        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; internal set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; internal set; }

        public Client(IPEndPoint _remoteEndPoint, DateTime _connectedAt, EClientState _clientState)
        {
            remoteEndPoint = _remoteEndPoint;
            connectedAt = _connectedAt;
            clientState = _clientState;
            commandIssued = String.Empty;

            BirthMark = LiveCache.GetUniqueIdentifier(String.Format(cacheKeyFormat, remoteEndPoint.Port));
            Birthdate = DateTime.Now;
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
