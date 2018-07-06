using Newtonsoft.Json;
using System;
using System.Configuration;
using System.Globalization;
using System.Reflection;

namespace NetMud.Gossip.Messaging
{
    [Serializable]
    public class Authentication : IPayload
    {
        [JsonIgnore]
        public string Type => "authenticate";

        [JsonProperty("supports")]
        public string[] FeaturesSupported => new string[] { "channels" };

        [JsonProperty("channels")]
        public string[] Channels => new string[] { "gossip" };

        [JsonProperty("client_id")]
        public string ClientId => ConfigurationManager.AppSettings["clientId"];

        [JsonProperty("client_secret")]
        public string ClientSecret => ConfigurationManager.AppSettings["clientSecret"];

        [JsonProperty("user_agent")]
        public string UserAgent
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format(CultureInfo.InvariantCulture, @"NetMud {0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
        }
    }
}
