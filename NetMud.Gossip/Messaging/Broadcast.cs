using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Broadcast : IPayload
    {
        [JsonIgnore]
        public string Type => "messages/broadcast";

        [JsonProperty("channel")]
        public string ChannelName { get; set; }

        [JsonProperty("name")]
        public string Username { get; set; }

        [JsonProperty("message")]
        public string MessageBody { get; set; }

        [JsonProperty("game")]
        public string GameName { get; set; }

        [JsonProperty("id")]
        public Guid MessageId { get; set; }
    }
}
