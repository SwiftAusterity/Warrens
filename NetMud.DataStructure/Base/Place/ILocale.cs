using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collection of rooms in a zone
    /// </summary>
    public interface ILocale : IEntity, ILocation, ISpawnAsSingleton
    {
        /// <summary>
        /// The room array that makes up the world
        /// </summary>
        IMap InteriorMap { get; }

        /// <summary>
        /// Get all the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        IEnumerable<IRoom> Rooms();

        /// <summary>
        /// The rooms that are also exits to zones
        /// </summary>
        /// <returns>Rooms</returns>
        Dictionary<IRoom, IZone> ZoneExitPoints();

        /// <summary>
        /// The rooms that are also exits to locales
        /// </summary>
        /// <returns>Rooms</returns>
        Dictionary<IRoom, IZone> LocaleExitPoints();


        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        IMap Interior { get; set; }

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoom CentralRoom(int zIndex = -1);

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
