using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class HeartbeatResponse : IPayload
    {
        [JsonIgnore]
        public string Type => "heartbeat";

        [JsonProperty("players")]
        public string[] Players { get; set; }
    }
}
