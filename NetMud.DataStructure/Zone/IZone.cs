using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    public interface IZone : IZoneFramework, IActor, IEnvironment, ISpawnAsSingleton<IZone>
    {
        /// <summary>
        /// The tile map for this zone
        /// </summary>
        IMap Map { get; }

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
        /// Pathways leading out of (or into) this
        /// </summary>
        IEnumerable<IPathway> GetPathways(bool inward = false);

        /// <summary>
        /// Gets the pathway if there is one at the coordinate
        /// </summary>
        /// <returns>The pathway (or null)</returns>
        IPathway GetPathway(long x, long y);

        /// <summary>
        /// Get any entities inside this
        /// </summary>
        /// <returns>Entity list</returns>
        IEnumerable<IEntity> GetContainedEntities();

        /// <summary>
        /// Returns whether or not the Actor has the right to alter things in this zone
        /// </summary>
        /// <param name="actor">the person acting</param>
        /// <returns>if they have the rights</returns>
        bool HasOwnershipRights(IActor actor);
    }
}
