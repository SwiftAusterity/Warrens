using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Highest order of "where am I"
    /// </summary>
    public interface IWorld : IData
    {
        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        IMap WorldMap { get; }

        /// <summary>
        /// Absolute full diameter for this entire world in meters
        /// </summary>
        long FullDiameter { get; set; }

        /// <summary>
        /// Strata collection of this world
        /// </summary>
        HashSet<IStratum> Strata { get; set; }

        /// <summary>
        /// Live chunk collection of this world
        /// </summary>
        HashSet<IChunk> Chunks { get; set; }
    }
}
