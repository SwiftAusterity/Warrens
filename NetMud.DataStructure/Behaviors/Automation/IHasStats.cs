using NetMud.DataStructure.Base.Supporting;
using System.Collections.Generic;
namespace NetMud.DataStructure.Behaviors.Automation
{
    /// <summary>
    /// Indicates an entity has RPG style stats (strength, etc)
    /// </summary>
    public interface IHasStats
    {
        /// <summary>
        /// Raw stat values
        /// </summary>
        Dictionary<IStat, int> Stats { get; }

        /// <summary>
        /// Gets the weighted value of the stat
        /// </summary>
        /// <param name="stat">the stat in question</param>
        /// <returns>the weighted value</returns>
        int GetWeightedStat(IStat stat);
    }
}
