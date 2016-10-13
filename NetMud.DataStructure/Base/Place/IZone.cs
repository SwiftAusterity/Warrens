using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZone : ILookupData
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        int PressureCoefficient { get; set; }

        /// <summary>
        /// Who currently owns this zone
        /// </summary>
        long Owner { get; set; } //long for now cause it's supposed to be guild/clan but that wont be implemented for a while, it'll be a bigint in the db anyways

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        bool Claimable { get; set; }

        /// <summary>
        /// Getall the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        IEnumerable<IRoomData> Rooms();

        /// <summary>
        /// Get the absolute center room of the zone
        /// </summary>
        /// <returns>the central room of the zone</returns>
        IRoomData CentralRoom();

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        string RenderMap();

        /// <summary>
        /// Gets the ascii render of all the rooms
        /// </summary>
        /// <returns></returns>
        string RenderRoomMap();

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count</returns>
        int Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>the dimensions of the zone in inches</returns>
        Tuple<int, int, int> FullDimensions();
    }
}
