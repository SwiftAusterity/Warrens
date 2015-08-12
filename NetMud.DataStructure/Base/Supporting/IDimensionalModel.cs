using NetMud.DataStructure.Base.System;
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

        /// <summary>
        /// Checks to see if we have enough planes and nodes to be a valid model
        /// </summary>
        /// <returns>validity</returns>
        bool IsModelValid();

        /// <summary>
        /// Gets a node based on a vertex
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <param name="yAxis">the Y-Axis of the node to get</param>
        /// <param name="zAxis">the Z-Axis of the node to get</param>
        /// <returns>the node</returns>
        IDimensionalModelNode GetNode(short xAxis, short yAxis, short zAxis);

        /// <summary>
        /// Gets the node behind the indicated node
        /// </summary>
        /// <param name="xAxis">the X-Axis of the initial node to get</param>
        /// <param name="yAxis">the Y-Axis of the initial node to get</param>
        /// <param name="zAxis">the Z-Axis of the initial node to get</param>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the node "behind" the node asked for (can be null)</returns>
        IDimensionalModelNode GetNodeBehindNode(short xAxis, short yAxis, short zAxis, short pitch, short yaw, short roll);

        /// <summary>
        /// View the flattened model based on view angle; TODO: ONLY SUPPORTS THE FRONT FACE ATM
        /// </summary>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the flattened model face based on the view angle</returns>
        string ViewFlattenedModel(short pitch, short yaw, short roll);
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

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
        /// </summary>
        short YAxis { get; set; }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <returns>the node</returns>
        IDimensionalModelNode GetNode(short xAxis, short zAxis);
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
        short ZAxis { get; set; }

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
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
