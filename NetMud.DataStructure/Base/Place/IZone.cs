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
        /// Is this zone ownership malleable
        /// </summary>
        bool Claimable { get; set; }

        /// <summary>
        /// The name it will confer to the world it loads to if it is the first zone to load a world
        /// </summary>
        string WorldName { get; set; }

        /// <summary>
        /// What world does this belong to (determined after load)
        /// </summary>
        IWorld World { get; set; }

        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        IMap ZoneMap { get; }

        /// <summary>
        /// Getall the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        IEnumerable<IRoomData> Rooms();

        /// <summary>
        /// Get the absolute center room of the zone
        /// </summary>
        /// <returns>the central room of the zone</returns>
        IRoomData CentralRoom(int zIndex = -1);

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        string RenderMap(int zIndex, bool forAdmin = false);

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count</returns>
        Tuple<int, int, int> Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>the dimensions of the zone in inches</returns>
        Tuple<int, int, int> FullDimensions();
    }
}
