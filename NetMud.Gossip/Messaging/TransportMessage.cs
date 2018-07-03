using Newtonsoft.Json;

namespace NetMud.Gossip.Messaging
{
    public class TransportMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }
    }
}
