using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    public class TransportMessage
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("ref")]
        public Guid ReferenceID { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("payload")]
        public dynamic Payload { get; set; }
    }
}
