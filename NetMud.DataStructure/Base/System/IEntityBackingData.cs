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
    }
}
