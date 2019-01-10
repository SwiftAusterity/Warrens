using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Locale;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IZone : IZoneFramework, IActor, ILocation, ISpawnAsSingleton<IZone>
    {
        /// <summary>
        /// Clouds, basically
        /// </summary>
        IEnumerable<IWeatherEvent> WeatherEvents { get; set; }

        /// <summary>
        /// Broadcast an event to the entire zone
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the sender</param>
        /// <param name="subject">the subject</param>
        /// <param name="target">the target</param>
        void BroadcastEvent(string message, IEntity sender = null, IEntity subject = null, IEntity target = null);

        /// <summary>
        /// Get the current forecast for this zone
        /// </summary>
        /// <returns>Bunch of stuff</returns>
        Tuple<PrecipitationAmount, PrecipitationType, HashSet<WeatherType>> CurrentForecast();

        /// <summary>
        /// Get the live world associated with this live zone
        /// </summary>
        /// <returns>The world</returns>
        IGaia GetWorld();

        /// <summary>
        /// Create a new randomized locale based on the template requested
        /// </summary>
        /// <param name="name">The name of the template requested, blank = use random</param>
        /// <returns>The locale generated</returns>
        ILocale GenerateAdventure(string templateName = "");
    }
}
