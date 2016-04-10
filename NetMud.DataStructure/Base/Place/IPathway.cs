
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Utility;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Entity for Pathways
    /// </summary>
    public interface IPathway : IActor, ISpawnAsSingleton
    {
        /// <summary>
        /// Location this pathway leads to
        /// </summary>
        ILocation ToLocation { get; set; }

        /// <summary>
        /// Location this pathway spawns into and leads away from
        /// </summary>
        ILocation FromLocation { get; set; }

        /// <summary>
        /// Cardinal direction this pathway is
        /// </summary>
        MovementDirectionType MovementDirection { get; }

        /// <summary>
        /// Message cluster for entities entering
        /// </summary>
        IMessageCluster Enter { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
