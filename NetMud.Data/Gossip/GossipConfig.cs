using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Reflection;
using System.Web.Script.Serialization;

namespace NetMud.Data.Gossip
{
    public class GossipConfig : ConfigData, IGossipConfig
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.GameWorld;

        /// <summary>
        /// Is gossip supposed to be on?
        /// </summary>
        [Display(Name = "Server Active", Description = "Is gossip supposed to be on?")]
        [UIHint("Boolean")]
        public bool GossipActive { get; set; }

        /// <summary>
        /// The ID for this gossip client
        /// </summary>
        [Display(Name = "Client Id", Description = "The ID for this gossip client.")]
        [DataType(DataType.Text)]
        public string ClientId { get; set; }

        /// <summary>
        /// The client secret to share for auth
        /// </summary>
        [Display(Name = "Client Secret", Description = "The client secret to share for auth.")]
        [DataType(DataType.Text)]
        public string ClientSecret { get; set; }

        /// <summary>
        /// The name this sends to gossip to represent itself
        /// </summary>
        [Display(Name = "Name", Description = "The name this sends to gossip to represent itself.")]
        [DataType(DataType.Text)]
        public string ClientName { get; set; }

        /// <summary>
        /// The version number
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string Version { get; set; }

        /// <summary>
        /// The useragent the client sends the gossip server
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string UserAgent
        {
            get
            {
                return string.Format("{0} {1}", ClientName, Version);
            }
            set
            {
                throw new NotImplementedException("Can't set user agent.");
            }
        }

        /// <summary>
        /// The maximum retry value
        /// </summary>
        [Display(Name = "Retry Loop Maximum", Description = "The maximum retry value. Higher = more retries.")]
        [Range(200, 1000, ErrorMessage = "Must be between 200 and 1000.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplierMaximum { get; set; }

        /// <summary>
        /// The multiplier for the retry value loop
        /// </summary>
        [Display(Name = "Retry Loop Multiplier", Description = "How much the retry loop monitor escalates each loop. Higher = less retries.")]
        [Range(1.15, 3, ErrorMessage = "Must be between 1.15 and 3.")]
        [DataType(DataType.Text)]
        public double SuspendMultiplier { get; set; }

        /// <summary>
        /// What channels we subscribe to initially
        /// </summary>
        [Display(Name = "Channels", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedChannels { get; set; }

        /// <summary>
        /// What features of gossip are supported
        /// </summary>
        [Display(Name = "Features", Description = "The name this sends to gossip to represent itself.")]
        [UIHint("TagContainer")]
        public HashSet<string> SupportedFeatures { get; set; }

        /// <summary>
        /// Setup with the default values
        /// </summary>
        public GossipConfig()
        {
            string clientName = "Warrens: White Sands";

            Version v = Assembly.GetExecutingAssembly().GetName().Version;
            Version = string.Format(CultureInfo.InvariantCulture, @"{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);

            Name = "GossipSettings";
            GossipActive = true;
            ClientName = clientName;
            SupportedChannels = new HashSet<string>() { "gossip", "warrensnet" };
            SupportedFeatures = new HashSet<string>() { "channels", "tells", "players", "games" };
            SuspendMultiplier = 1.15;
            SuspendMultiplierMaximum = 500;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GossipConfig
            {
                Name = Name,
                GossipActive = GossipActive,
                ClientId = ClientId,
                ClientName = ClientName,
                ClientSecret = ClientSecret,
                SupportedChannels = SupportedChannels,
                SupportedFeatures = SupportedFeatures,
                SuspendMultiplier = SuspendMultiplier,
                SuspendMultiplierMaximum = SuspendMultiplierMaximum
            };
        }
    }
}
