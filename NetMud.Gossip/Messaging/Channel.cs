using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Channel : IPayload
    {
        [JsonIgnore]
        public string Type => "channel";

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }
    }
}
