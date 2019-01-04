using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        public bool GossipActive { get; set; }

        /// <summary>
        /// The ID for this gossip client
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// The client secret to share for auth
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// The name this sends to gossip to represent itself
        /// </summary>
        public string ClientName { get; set; }

        /// <summary>
        /// The version number
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string Version
        {
            get
            {
                Version v = Assembly.GetExecutingAssembly().GetName().Version;
                return string.Format(CultureInfo.InvariantCulture, @"{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            }
            set
            {
                throw new NotImplementedException("Can't set version.");
            }
        }

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
        public double SuspendMultiplierMaximum { get; set; }

        /// <summary>
        /// The multiplier for the retry value loop
        /// </summary>
        public double SuspendMultiplier { get; set; }

        /// <summary>
        /// What channels we subscribe to initially
        /// </summary>
        public HashSet<string> SupportedChannels { get; set; }

        /// <summary>
        /// What features of gossip are supported
        /// </summary>
        public HashSet<string> SupportedFeatures { get; set; }

        /// <summary>
        /// Setup with the default values
        /// </summary>
        public GossipConfig()
        {
            string clientName = "Warrens: Leaf Green";

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
