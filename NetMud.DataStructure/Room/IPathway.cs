using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Entity for Pathways
    /// </summary>
    public interface IPathway : IPathwayFramework, IActor, ISpawnAsSingleton<IPathway>
    {
        /// <summary>
        /// Message cluster for entities entering
        /// </summary>
        IMessageCluster Enter { get; set; }

        /// <summary>
        /// The container this points into
        /// </summary>
        ILocation Destination { get; set; }

        /// <summary>
        /// The container this starts in
        /// </summary>
        ILocation Origin { get; set; }
    }
}
