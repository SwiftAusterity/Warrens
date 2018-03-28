using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// The one location that isn't a location. Contains rooms
    /// </summary>
    public interface ILocale : IActor, IDiscoverable, IHomesteading, ISpawnAsSingleton
    {
        /// <summary>
        /// When this expires. MaxDate = never essentially for purposefully built locales
        /// </summary>
        DateTime Expiration { get; set; }

        /// <summary>
        /// The rooms contained within the locale
        /// </summary>
        HashSet<IRoom> Rooms { get; set; }

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
        /// The diameter of the locale
        /// </summary>
        /// <returns>the diameter of the locale in room count</returns>
        Tuple<int, int, int> Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the locale in inches
        /// </summary>
        /// <returns>the dimensions of the locale in inches</returns>
        Tuple<int, int, int> FullDimensions();
    }
}
