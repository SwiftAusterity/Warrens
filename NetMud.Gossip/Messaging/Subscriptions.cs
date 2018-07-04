using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Subscriptions : IPayload
    {
        [JsonIgnore]
        public string Type => "channels/subscribed";

        [JsonProperty("channels")]
        public string[] SubscribedChannels { get; set; }
    }
}
