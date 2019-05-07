using NetMud.DataStructure.Architectural.ActorBase;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition : ICloneable
    {
        /// <summary>
        /// What section of the room are you in
        /// </summary>
        ulong CurrentSection { get; set; }

        /// <summary>
        /// Get entities in this section and at a radius
        /// </summary>
        /// <param name="radius">radius to search within</param>
        /// <returns>the list of entities</returns>
        IEnumerable<IActor> GetContents(ulong radius);
    }
}
