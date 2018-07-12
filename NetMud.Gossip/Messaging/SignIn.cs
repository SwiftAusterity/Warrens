using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class SignIn : IPayload
    {
        [JsonIgnore]
        public string Type => "players/sign-in";

        [JsonProperty("name")]
        public string PlayerName { get; set; }
    }
}
