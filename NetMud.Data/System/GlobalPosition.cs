using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    [Serializable]
    public class GlobalPosition : IGlobalPosition
    {
        /// <summary>
        /// Birthmark for current live location of this
        /// </summary>
        [JsonProperty("CurrentLocation")]
        public LiveCacheKey CurrentLocationBirthmark { get; private set; }

        [JsonIgnore]
        [ScriptIgnore]
        public IContains CurrentLocation
        {
            get
            {
                if (!String.IsNullOrWhiteSpace(CurrentLocationBirthmark?.BirthMark))
                {
                    return LiveCache.Get<IContains>(CurrentLocationBirthmark);
                }

                return null;
            }
            set
            {
                if (value == null)
                    return;

                CurrentLocationBirthmark = new LiveCacheKey(value.GetType(), value.BirthMark);
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

        public GlobalPosition(IContains currentLocation)
        {
            CurrentLocation = currentLocation;
        }
    }
}
