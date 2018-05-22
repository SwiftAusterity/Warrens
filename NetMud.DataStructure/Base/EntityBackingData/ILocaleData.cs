using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleData : IEntityBackingData, IDiscoverableData, ISingleton<ILocale>
    {
        /// <summary>
        /// When this locale dies off, MinValue = never
        /// </summary>
        DateTime RollingExpiration { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        IZoneData ParentLocation { get; set; }

        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        IMap Interior { get; set; }

        /// <summary>
        /// Regenerate the internal map for the locale
        /// </summary>
        void RemapInterior();

        /// <summary>
        /// The rooms contained within the locale should it need to regenerate from nothing
        /// </summary>
        IEnumerable<IRoomData> Rooms();

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoomData CentralRoom(int zIndex = -1);

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
