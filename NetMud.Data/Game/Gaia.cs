using NetMud.Data.EntityBackingData;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Existential;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Collector of locales, used for weather and herd patterning
    /// </summary>
    [Serializable]
    public class Gaia : EntityPartial, IGaia
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string DataTemplateName
        {
            get
            {
                return DataTemplate<IGaiaData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IGaiaData), DataTemplateId));
        }

        /// <summary>
        /// The current time of day (and month and year)
        /// </summary>
        public ITimeOfDay CurrentTimeOfDay { get; set; }

        /// <summary>
        /// Collection of weather patterns for this world
        /// </summary>
        public IEnumerable<IWeatherPattern> MeterologicalFronts { get; set; }

        /// <summary>
        /// Economic controller for this world
        /// </summary>
        public IEconomy Macroeconomy { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        public IEnumerable<Tuple<ICelestial, float>> CelestialPositions { get; set; }

        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IGaia>(DataTemplateId, typeof(GaiaData));

            //Isn't in the world currently
            if (me == default(IGaia))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Keywords = me.Keywords;
                CurrentLocation = null;
                CurrentTimeOfDay = me.CurrentTimeOfDay;
                MeterologicalFronts = me.MeterologicalFronts;
                Macroeconomy = me.Macroeconomy;
                CelestialPositions = me.CelestialPositions;
            }
        }

        public IGaia GetLiveInstance()
        {
            return this;
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            return DataTemplate<IGaiaData>().GetModelDimensions();
        }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        public IEnumerable<IZone> GetZones()
        {
            return LiveCache.GetAll<IZone>().Where(zone => zone.GetWorld().Equals(this));
        }

        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(null);
        }

        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<IGaiaData>() ?? throw new InvalidOperationException("Missing backing data store on gaia spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };

            if (CelestialPositions == null)
                CelestialPositions = new List<Tuple<ICelestial, float>>();

            if (MeterologicalFronts == null)
                MeterologicalFronts = Enumerable.Empty<IWeatherPattern>();

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);
        }
    }
}
