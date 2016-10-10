using System.Collections.Generic;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.Place;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomData : IEntityBackingData
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
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        IEnumerable<IPathwayData> GetPathways();
    }
}
