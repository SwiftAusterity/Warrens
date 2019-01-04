namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// player-to-player connections
    /// </summary>
    public interface IAcquaintence
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
        AcquaintenceNotifications[] NotificationSubscriptions { get; set; }
    }
}
