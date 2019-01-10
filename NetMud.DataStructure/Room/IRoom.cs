using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Locale;
using System;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Entity for Rooms
    /// </summary>
    public interface IRoom : IRoomFramework, IActor, ILocation, ISpawnAsSingleton<IRoom>
    {
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
        Tuple<int, IPathway> GetDistanceAndNextStepDestination(ILocation destination);

        /// <summary>
        /// What locale does this belong to
        /// </summary>
        ILocale ParentLocation { get; set; }
    }
}
