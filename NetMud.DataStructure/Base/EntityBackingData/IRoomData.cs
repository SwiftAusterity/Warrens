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

        IDictionary<string, IMaterial> Borders { get; set; }

        IMaterial Medium { get; set; }

        IZone ZoneAffiliation { get; set; }

        string SerializeBorders();
    }
}
