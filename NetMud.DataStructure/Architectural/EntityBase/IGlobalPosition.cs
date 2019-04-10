using System;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition : ICloneable
    {
        ulong CurrentSlice { get; set; }
    }
}
