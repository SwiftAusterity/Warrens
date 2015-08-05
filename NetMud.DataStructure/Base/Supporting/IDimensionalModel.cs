using NetMud.DataStructure.Base.System;
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
        /// The model we're following
        /// </summary>
        IDimensionalModelData Model { get; set; }
    }

    /// <summary>
    /// Backing data for physical models
    /// </summary>
    public interface IDimensionalModelData : IReferenceData
    {
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
        HashSet<IDimensionalModelNode> ModelNodes { get; set; }

        /// <summary>
        /// The name of this plane (for a sword it might be 'Blade' or 'Hilt')
        /// </summary>
        string TagName { get; set; }
    }

    /// <summary>
    /// A single node of the 121 nodes per plane in the dimensional model
    /// </summary>
    public interface IDimensionalModelNode
    {
        /// <summary>
        /// The position of this node on the XAxis
        /// </summary>
        short XAxis { get; set; }

        /// <summary>
        /// The position of this node on the YAxis
        /// </summary>
        short YAxis { get; set; }
        
        /// <summary>
        /// The damage type inflicted when this part of the model strikes
        /// </summary>
        DamageType Style { get; set; }
        
        /// <summary>
        /// Material composition of the node
        /// </summary>
        IMaterial Composition { get; set; }
    }
}
