using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Locale
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleTemplate : ILocaleFramework, ITemplate, IDiscoverableData, ISingleton<ILocale>
    {
        /// <summary>
        /// When this locale dies off, MinValue = never
        /// </summary>
        DateTime RollingExpiration { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        IZoneTemplate ParentLocation { get; set; }

        /// <summary>
        /// Regenerate the internal map for the locale
        /// </summary>
        void RemapInterior();

        /// <summary>
        /// The rooms contained within the locale should it need to regenerate from nothing
        /// </summary>
        IEnumerable<IRoomTemplate> Rooms();

        /// <summary>
        /// Get the absolute center room of the locale
        /// </summary>
        /// <returns>the central room of the locale</returns>
        IRoomTemplate CentralRoom(int zIndex = -1);
    }
}
