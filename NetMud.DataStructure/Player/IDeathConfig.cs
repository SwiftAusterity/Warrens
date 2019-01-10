using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Config to handle player death
    /// </summary>
    public interface IDeathConfig
    {
        /// <summary>
        /// The zone you return to on death
        /// </summary>
        IZoneTemplate DeathRecallZone { get; set; }

        /// <summary>
        /// The coordinates you recall to on death
        /// </summary>
        Coordinate DeathRecallCoordinates { get; set; }

        /// <summary>
        /// The subject of the death notice notification sent on death
        /// </summary>
        string DeathNoticeSubject { get; set; }

        /// <summary>
        /// The from field for the death notice
        /// </summary>
        string DeathNoticeFrom { get; set; }

        /// <summary>
        /// The body of the death notice
        /// </summary>
        MarkdownString DeathNoticeBody { get; set; }

        /// <summary>
        /// Should any qualities of the player change on death (like money removal)
        /// </summary>
        HashSet<QualityValue> QualityChanges { get; set; }
    }
}
