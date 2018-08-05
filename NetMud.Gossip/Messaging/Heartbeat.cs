using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Heartbeat : IPayload
    {
        [JsonIgnore]
        public string Type => "heartbeat";
    }
}
