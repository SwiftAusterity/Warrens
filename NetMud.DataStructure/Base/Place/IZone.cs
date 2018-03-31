using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IZone : IActor, ILocation, IDiscoverable, ISpawnAsSingleton
    {
        /// <summary>
        /// Locales within this zone
        /// </summary>
        HashSet<ILocale> Locales { get; set; }

        /// <summary>
        /// Create a new randomized locale based on the template requested
        /// </summary>
        /// <param name="name">The name of the template requested, blank = use random</param>
        /// <returns>The locale generated</returns>
        ILocale GenerateAdventure(string name = "");

        /// <summary>
        /// Get the zones this exits to (factors in visibility)
        /// </summary>
        IEnumerable<IZone> ZoneExits(IEntity viewer);
    }
}
