using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomData : ILocationData, IDescribable
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }

        /// <summary>
        /// What the room's primary material is (is it filled with water, air, etc)
        /// </summary>
        IMaterial Medium { get; set; }

        /// <summary>
        /// What locale does this belong to
        /// </summary>
        ILocaleData ParentLocation { get; set; }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>
        Tuple<int, int, int> Coordinates { get; set; }

        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        int GetDistanceDestination(ILocationData destination);
    }
}
