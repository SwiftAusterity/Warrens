using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Automation;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Mid-point Interface for entity backing data
    /// </summary>
    public interface IEntityBackingData : IData, IHasAffects
    {
        /// <summary>
        /// Entity class this backing data attaches to
        /// </summary>
        Type EntityClass { get; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Tuple<int, int, int> GetModelDimensions();
    }
}
