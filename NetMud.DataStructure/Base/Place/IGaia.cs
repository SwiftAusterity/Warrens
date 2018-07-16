using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IGaia : IEntity, ISpawnAsSingleton<IGaia>
    {
        /// <summary>
        /// The current time of day (and month and year)
        /// </summary>
        ITimeOfDay CurrentTimeOfDay { get; set; }

        /// <summary>
        /// Where the planet is rotationally
        /// </summary>
        float PlanetaryRotation { get; set; }

        /// <summary>
        /// Where the planet is in its orbit
        /// </summary>
        float OrbitalPosition { get; set; }

        /// <summary>
        /// Collection of weather patterns for this world
        /// </summary>
        IEnumerable<IWeatherPattern> MeterologicalFronts { get; set; }

        /// <summary>
        /// Economic controller for this world
        /// </summary>
        IEconomy Macroeconomy { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        IEnumerable<Tuple<ICelestial, float>> CelestialPositions { get; set; }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        IEnumerable<IZone> GetZones();
    }
}
