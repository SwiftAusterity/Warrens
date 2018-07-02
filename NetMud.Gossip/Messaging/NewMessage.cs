using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    [JsonObject(Id ="messages/new")]
    public class NewMessage
    {
        [JsonProperty("channel")]
        public string ChannelName { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("message")]
        public string MessageBody { get; set; }
    }
}
