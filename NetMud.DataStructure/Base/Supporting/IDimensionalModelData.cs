using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Backing data for physical models
    /// </summary>
    public interface IDimensionalModelData : ILookupData
    {
        /// <summary>
        /// Governs what sort of model planes we're looking for
        /// </summary>
        DimensionalModelType ModelType { get; set; }

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
        /// View the flattened model based on view angle
        /// </summary>
        /// <param name="pitch">rotation on the z-axis</param>
        /// <param name="yaw">rotation on the Y-axis</param>
        /// <param name="roll">rotation on the x-axis</param>
        /// <returns>the flattened model face based on the view angle</returns>
        string ViewFlattenedModel(short pitch, short yaw, short roll);
    }

    /// <summary>
    /// Governs what sort of model planes we're looking for, numerical value represents how many dimensions the model has
    /// </summary>
    public enum DimensionalModelType : short
    {
        None = 0,
        Flat = 2,
        ThreeD = 3
    }
}
