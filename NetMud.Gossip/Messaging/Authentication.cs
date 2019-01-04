using Newtonsoft.Json;
using System;
using System.Linq;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Authentication : IPayload
    {
        [JsonIgnore]
        public string Type => "authenticate";

        [JsonProperty("supports")]
        public string[] FeaturesSupported { get; }

        [JsonProperty("channels")]
        public string[] Channels { get; }

        [JsonProperty("client_id")]
        public string ClientId { get; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; }

        [JsonProperty("version")]
        public string Version { get; }

        [JsonProperty("user_agent")]
        public string UserAgent { get; }

        public Authentication(IConfig config)
        {
            ClientId = config.ClientId;
            ClientSecret = config.ClientSecret;

            Channels = config.SupportedChannels.ToArray();
            FeaturesSupported = config.SupportedFeatures.ToArray();
            UserAgent = config.UserAgent;
            Version = config.Version;
        }
    }
}
