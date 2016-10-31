using System.Collections.Generic;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.Place;
using System;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomData : IEntityBackingData, ISingleton
    {
        /// <summary>
        /// The current physical model for this entity
        /// </summary>
        IDimensionalModel Model { get; }

        /// <summary>
        /// What the walls are made of
        /// </summary>
        IDictionary<string, IMaterial> Borders { get; set; }

        /// <summary>
        /// What the room's primary material is (is it filled with water, air, etc)
        /// </summary>
        IMaterial Medium { get; set; }

        /// <summary>
        /// What zone does this belong to
        /// </summary>
        IZone ZoneAffiliation { get; set; }

        /// <summary>
        /// Current coordinates of the room on its world map
        /// </summary>
        Tuple<int, int, int> Coordinates { get; set; }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        IEnumerable<IPathwayData> GetPathways(bool withReturn = false);

        /// <summary>
        /// Gets the remaining distance to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) between here and there</returns>
        int GetDistanceToRoom(IRoomData destination);
    }
}
