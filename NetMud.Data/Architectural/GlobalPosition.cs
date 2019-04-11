using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural
{
    /// <summary>
    /// An entity's position in the world
    /// </summary>
    [Serializable]
    public class GlobalPosition : IGlobalPosition
    {
        /// <summary>
        /// What section of the room are you in
        /// </summary>
        public ulong CurrentSection { get; set; }

        /// <summary>
        /// This constructor is required for the JSON deserializer to be able
        /// to identify concrete classes to use when deserializing the interface properties.
        /// </summary>
        [JsonConstructor]
        public GlobalPosition()
        {
            CurrentSection = 0;
        }

        /// <summary>
        /// Construct with the zone to set as the location
        /// </summary>
        /// <param name="currentLocation">the container</param>
        public GlobalPosition(ulong currentSection)
        {
            CurrentSection = currentSection;
        }


        /// <summary>
        /// Get entities in this section and at a radius
        /// </summary>
        /// <param name="radius">radius to search within</param>
        /// <returns>the list of entities</returns>
        public IEnumerable<IEntity> GetContents(ulong radius)
        {
            return LiveCache.GetAll<IPlayer>().Where(player => player.CurrentLocation.CurrentSection.IsBetweenOrEqual(
                                                    Math.Max(ulong.MinValue, CurrentSection - radius), Math.Min(ulong.MaxValue, CurrentSection + radius)));
        }

        /// <summary>
        /// make a copy of this
        /// </summary>
        /// <returns>a clone of this</returns>
        public object Clone()
        {
            return new GlobalPosition(CurrentSection);
        }
    }
}
