using NetMud.DataStructure.Base.Supporting;
using System;

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
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Tuple<int, int, int> GetModelDimensions();
    }
}
