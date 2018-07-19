using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class NewMessage : IPayload
    {
        [JsonIgnore]
        public string Type => "channels/send";

        [JsonProperty("channel")]
        public string ChannelName { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("message")]
        public string MessageBody { get; set; }
    }

    [Serializable]
    public class NewDirectMessage : IPayload
    {
        [JsonIgnore]
        public string Type => "tells/send";

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("target")]
        public string Target { get; set; }

        [JsonProperty("message")]
        public string MessageBody { get; set; }
    }
}
