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

    [Serializable]
    public class HeartbeatResponse : IPayload
    {
        [JsonIgnore]
        public string Type => "heartbeat";

        [JsonProperty("players")]
        public string[] Players { get; set; }
    }
}
