using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Entity
{
    /// <summary>
    /// "Object" entity
    /// </summary>
    public interface IInanimate : IActor, ILocation, ISpawnAsMultiple
    {
        /// <summary>
        /// Inventory of the object, essentially
        /// </summary>
        IEntityContainer<IInanimate> Contents { get; set; }

        /// <summary>
        /// Last known location ID for the object in the real world
        /// </summary>
        long LastKnownLocation { get; set; }

        /// <summary>
        /// System type for the last known location for the object in the real world
        /// </summary>
        string LastKnownLocationType { get; set; }

        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }
    }
}
