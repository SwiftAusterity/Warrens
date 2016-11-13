using NetMud.DataStructure.Base.Supporting;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Mid-point Interface for entity backing data
    /// </summary>
    public interface IEntityBackingData : IData
    {
        /// <summary>
        /// Entity class this backing data attaches to
        /// </summary>
        Type EntityClass { get; }

        /// <summary>
        /// Affects to add to a live entity when it is spawned
        /// </summary>
        HashSet<IAffect> AffectsOnSpawn { get; set; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Tuple<int, int, int> GetModelDimensions();
    }
}
