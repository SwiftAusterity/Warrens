using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Unsubscribe : IPayload
    {
        [JsonIgnore]
        public string Type => "channels/unsubscribe";

        [JsonProperty("channel")]
        public string Channel { get; set; }
    }
}
