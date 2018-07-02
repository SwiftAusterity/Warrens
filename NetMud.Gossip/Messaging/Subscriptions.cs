using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    [JsonObject(Id = "channels/subscribed")]
    public class Subscriptions
    {
        [JsonProperty("channels")]
        public string[] SubscribedChannels { get; set; }
    }
}
