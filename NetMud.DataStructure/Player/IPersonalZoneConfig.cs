using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Config settings for how the personal maps get spawned
    /// </summary>
    public interface IPersonalZoneConfig
    {
        /// <summary>
        /// The zone data to base this one on
        /// </summary>
        IZoneTemplate Basis { get; set; }

        /// <summary>
        /// Random animals to put in
        /// </summary>
        HashSet<INPCRepop> WildAnimals { get; set; }
    }
}
