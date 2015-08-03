using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Entity for Rooms
    /// </summary>
    public interface IRoom : IActor, ILocation, ISpawnAsSingleton
    {
        /// <summary>
        /// Inanimates in the room (on the floor)
        /// </summary>
        IEntityContainer<IInanimate> ObjectsInRoom { get; set; }

        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        IEntityContainer<IMobile> MobilesInRoom { get; set; }

        /// <summary>
        /// Pathways leading from this room
        /// </summary>
        IEntityContainer<IPathway> Pathways { get; set; }
    }
}
