using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Framework for the physics model of an entity
    /// </summary>
    public interface IDimensionalModel
    {
        /// <summary>
        /// Y axis of the 11 plane model
        /// </summary>
        int Length { get; set; }

        /// <summary>
        /// Measurement of all 11 planes vertically
        /// </summary>
        int Height { get; set; }

        /// <summary>
        /// X axis of the 11 plane model
        /// </summary>
        int Width { get; set; }

        /// <summary>
        /// The 11 planes that compose the physical model
        /// </summary>
        HashSet<IDimensionalModelPlane> ModelPlanes { get; set; }
    }

    /// <summary>
    /// A single 11x11 grid of the 11 planes that compose the dimensional model
    /// </summary>
    public interface IDimensionalModelPlane
    {
        /// <summary>
        /// The collection of 121 nodes in the plane
        /// </summary>
        HashSet<Tuple<short, short, DamageType, IMaterial>> ModelNodes { get; set; }
    }
}
