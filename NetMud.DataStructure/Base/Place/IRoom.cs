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
    }
}
