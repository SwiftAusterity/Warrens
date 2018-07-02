using Newtonsoft.Json;
using System;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    [JsonObject(Id = "game")]
    public class Game
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }

        [JsonProperty("password_confirmation")]
        public string PasswordConfirmation { get; set; }

        [JsonProperty("password_hash")]
        public string PasswordHash { get; set; }

        [JsonProperty("user_agent")]
        public string UserAgent { get; set; }

        [JsonProperty("token")]
        public Guid Token { get; set; }

        [JsonProperty("client_id")]
        public Guid ClientId { get; set; }

        [JsonProperty("client_secret")]
        public Guid ClientSecret { get; set; }
    }
}
