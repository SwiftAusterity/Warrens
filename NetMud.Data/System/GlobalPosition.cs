using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    /// <summary>
    /// An entity's position in the world
    /// </summary>
    [Serializable]
    public class GlobalPosition : IGlobalPosition
    {
        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonProperty("CurrentLocation")]
        public LiveCacheKey CurrentLocationBirthmark { get; set; }

        /// <summary>
        /// The actual container that the current location is
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IContains CurrentLocation
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(CurrentLocationBirthmark?.BirthMark))
                {
                    return (IContains)LiveCache.Get(CurrentLocationBirthmark);
                }

                return null;
            }
            set
            {
                if (value == null)
                    return;

                CurrentLocationBirthmark = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// This constructor is required for the JSON deserializer to be able
        /// to identify concrete classes to use when deserializing the interface properties.
        /// </summary>
        [JsonConstructor]
        public GlobalPosition(LiveCacheKey locationKey)
        {
            CurrentLocationBirthmark = locationKey;
        }

        /// <summary>
        /// Construct with the container to set as the location
        /// </summary>
        /// <param name="currentLocation">the container</param>
        public GlobalPosition(IContains currentLocation)
        {
            CurrentLocation = currentLocation;
        }

        /// <summary>
        /// The zone of the current location
        /// </summary>
        /// <returns>The zone</returns>
        public IZone GetZone()
        {
            if (CurrentLocation.GetType().GetInterfaces().Contains(typeof(IZone)))
                return (IZone)CurrentLocation;

            if(CurrentLocation.GetType().GetInterfaces().Contains(typeof(IRoom)))
            {
                var room = (IRoom)CurrentLocation;
                return room.ParentLocation.ParentLocation;
            }

            return null;
        }

        /// <summary>
        /// The locale of the current Location
        /// </summary>
        /// <returns>The locale, might be null</returns>
        public ILocale GetLocale()
        {
            if (CurrentLocation.GetType().GetInterfaces().Contains(typeof(IRoom)))
            {
                var room = (IRoom)CurrentLocation;
                return room.ParentLocation;
            }

            return null;
        }

        /// <summary>
        /// The room of the current location
        /// </summary>
        /// <returns>The room, might be null</returns>
        public IRoom GetRoom()
        {
            if (CurrentLocation.GetType().GetInterfaces().Contains(typeof(IRoom)))
                return (IRoom)CurrentLocation;

            return null;
        }
    }
}
