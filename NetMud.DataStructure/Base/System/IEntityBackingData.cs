using NetMud.DataStructure.Behaviors.Automation;
using System;

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
        /// Keywords this entity can be found with in command parsing (needed for admin commands that look for data)
        /// </summary>
        string[] Keywords { get; set; }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        Tuple<int, int, int> GetModelDimensions();
    }
}
