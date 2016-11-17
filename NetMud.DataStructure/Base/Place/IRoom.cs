using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Base.Supporting;
using System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Entity for Rooms
    /// </summary>
    public interface IRoom : IActor, ILocation, ISpawnAsSingleton
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        string RenderCenteredMap(int radius, bool visibleOnly);

        /// <summary>
        /// Gets the remaining distance and next "step" to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) and the next path you'd have to use</returns>
        Tuple<int, IPathway> GetDistanceAndNextStepToRoom(IRoom destination);
    }
}
