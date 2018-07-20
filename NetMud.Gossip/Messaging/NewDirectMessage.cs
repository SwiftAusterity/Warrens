using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class NewDirectMessage : IPayload
    {
        [JsonIgnore]
        public string Type => "tells/send";

        [JsonProperty("from")]
        public string Username { get; set; }

        [JsonProperty("game")]
        public string Gamename { get; set; }

        [JsonProperty("player")]
        public string Target { get; set; }

        //2018-07-17T13:12:28Z
        [JsonProperty("sent_at")]
        public string Sent => DateTime.UtcNow.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'Z'");

        [JsonProperty("message")]
        public string MessageBody { get; set; }
    }
}
