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

        /// <summary>
        /// Keywords this entity can be found with in command parsing (needed for admin commands that look for data)
        /// </summary>
        string[] Keywords { get; set; }
    }
}
