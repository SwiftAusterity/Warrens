using System.Collections.Generic;

namespace NetMud.Gossip
{
    /// <summary>
    /// Global settings for the entire system
    /// </summary>
    public interface IConfig
    {
        /// <summary>
        /// Is gossip supposed to be on?
        /// </summary>
        bool GossipActive { get; set; }

        /// <summary>
        /// The ID for this gossip client
        /// </summary>
        string ClientId { get; set; }

        /// <summary>
        /// The client secret to share for auth
        /// </summary>
        string ClientSecret { get; set; }

        /// <summary>
        /// The name this sends to gossip to represent itself
        /// </summary>
        string ClientName { get; set; }

        /// <summary>
        /// The user agent to submit to the gossip server
        /// </summary>
        string UserAgent { get; set; }

        /// <summary>
        /// Version number for this game
        /// </summary>
        string Version { get; set; }

        /// <summary>
        /// The maximum retry value
        /// </summary>
        double SuspendMultiplierMaximum { get; set; }

        /// <summary>
        /// The multiplier for the retry value loop
        /// </summary>
        double SuspendMultiplier { get; set; }

        /// <summary>
        /// What channels we subscribe to initially
        /// </summary>
        HashSet<string> SupportedChannels { get; set; }

        /// <summary>
        /// What features of gossip are supported
        /// </summary>
        HashSet<string> SupportedFeatures { get; set; }
    }
}
