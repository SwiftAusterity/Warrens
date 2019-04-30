using System;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Mid-point Interface for entity backing data
    /// </summary>
    public interface ITemplate : IKeyedData
    {
        /// <summary>
        /// Entity class this backing data attaches to
        /// </summary>
        Type EntityClass { get; }
    }
}
