using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleData : IEntityBackingData, ISingleton
    {
        /// <summary>
        /// The zone this belongs to
        /// </summary>
        IZoneData Zone { get; set; }

        /// <summary>
        /// Does this locale require being found before it's visible in the zone
        /// </summary>
        bool Discoverable { get; set; }

        /// <summary>
        /// Zones this can exit to
        /// </summary>
        IEnumerable<IZoneData> ZoneExits { get; set; }

        /// <summary>
        /// Locales this can exit to
        /// </summary>
        IEnumerable<ILocaleData> LocaleExits { get; set; }

        /// <summary>
        /// Getall the rooms for the zone
        /// </summary>
        /// <returns>the rooms for the zone</returns>
        IEnumerable<IRoomData> Rooms { get; set; }
    }
}
