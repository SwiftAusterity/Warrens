using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface IGossipClient
    {
        string CacheKey { get; }
        void Launch();
        void Shutdown();
        void SendMessage(string userName, string messageBody, string channel = "gossip");
    }
}
