using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IZone : IActor, ILocation, IDiscoverable, ISpawnAsSingleton<IZone>
    {
        /// <summary>
        /// Create a new randomized locale based on the template requested
        /// </summary>
        /// <param name="name">The name of the template requested, blank = use random</param>
        /// <returns>The locale generated</returns>
        ILocale GenerateAdventure(string templateName = "");
    }
}
