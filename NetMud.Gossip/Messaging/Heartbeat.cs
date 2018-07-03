using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Heartbeat : IPayload
    {
        public string Type => "heartbeat";
    }
}
