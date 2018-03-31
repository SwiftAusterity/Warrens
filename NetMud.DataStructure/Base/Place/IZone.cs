using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZone : IEntity, IEnvironment
    {
        /// <summary>
        /// The locales active in this zone
        /// </summary>
        IEnumerable<ILocale> Locales();

        /// <summary>
        /// Get the zones this exits to (factors in visibility)
        /// </summary>
        IEnumerable<IZone> ZoneExits();
    }
}
