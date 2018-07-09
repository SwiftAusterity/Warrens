using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.PlayerConfiguration
{
    /// <summary>
    /// player-to-player connections
    /// </summary>
    public interface IAcquaintance
    {
        /// <summary>
        /// The account handle of the person involved
        /// </summary>
        string PersonHandle { get; set; }

        /// <summary>
        /// The account in question
        /// </summary>
        IAccount Person {get; set; }

        /// <summary>
        /// Is this a friend or foe?
        /// </summary>
        bool IsFriend { get; set; }

        /// <summary>
        /// Is this person a gossip account? the Person will be null then.
        /// </summary>
        bool GossipSystem { get; set; }

        /// <summary>
        /// What notifications are you subscribed to for this person
        /// </summary>
        AcquaintanceNotifications[] NotificationSubscriptions { get; set; }
    }
}
