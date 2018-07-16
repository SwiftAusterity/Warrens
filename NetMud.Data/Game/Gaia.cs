using NetMud.CentralControl;
using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
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
        [ScriptIgnore] //TODO: Stop ignoring weather and economy once these are figured out
        [JsonIgnore]
        public IEnumerable<IWeatherPattern> MeterologicalFronts { get; set; }

        /// <summary>
        /// Economic controller for this world
        /// </summary>
        [ScriptIgnore] //TODO: Stop ignoring weather and economy once these are figured out
        [JsonIgnore]
        public IEconomy Macroeconomy { get; set; }

        [JsonProperty("CelestialPositions")]
        public IEnumerable<Tuple<BackingDataCacheKey, float>> _celestialPositions { get; set; }

        /// <summary>
        /// Where the various celestial bodies are along their paths
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<Tuple<ICelestial, float>> CelestialPositions
        {
            get
            {
                if (_celestialPositions == null)
                    _celestialPositions = Enumerable.Empty<Tuple<BackingDataCacheKey, float>>();

                return _celestialPositions.Select(cp => new Tuple<ICelestial, float>(BackingDataCache.Get<ICelestial>(cp.Item1), cp.Item2));
            }
            set
            {
                if (value == null)
                    return;

                _celestialPositions = value.Select(cp => new Tuple<BackingDataCacheKey, float>(new BackingDataCacheKey(cp.Item1), cp.Item2));
            }
        }

        public Gaia()
        {
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Gaia(IGaiaData world)
        {
            DataTemplateId = world.Id;

            GetFromWorldOrSpawn();
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            return DataTemplate<IGaiaData>().GetModelDimensions();
        }

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

                KickoffProcesses();
            }
        }

        public IGaia GetLiveInstance()
        {
            return this;
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

            if (CelestialPositions == null || CelestialPositions.Count() == 0)
            {
                var celestials = new List<Tuple<ICelestial, float>>();

                foreach (var body in bS.CelestialBodies)
                    celestials.Add(new Tuple<ICelestial, float>(body, 0));

                CelestialPositions = celestials;
            }

            if (MeterologicalFronts == null || MeterologicalFronts.Count() == 0)
                MeterologicalFronts = Enumerable.Empty<IWeatherPattern>();

            CurrentTimeOfDay = new TimeOfDay(bS.ChronologicalSystem);

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
        }

        private void KickoffProcesses()
        {
            //every 15 minutes after half an hour
            Processor.StartSubscriptionLoop("Time", () => AdvanceTime(), 5 * 60, false);

            //every 15 minutes after half an hour
            Processor.StartSubscriptionLoop("CelestialBodies", () => AdvanceCelestials(), 5 * 60, false);
        }

        private bool AdvanceTime()
        {
            CurrentTimeOfDay.AdvanceByHour();
            Save();

            return true;
        }

        private bool AdvanceCelestials()
        {
            var newCelestials = new List<Tuple<ICelestial, float>>();
            foreach (var celestial in CelestialPositions)
            {
                if (celestial.Item1.OrientationType == CelestialOrientation.HelioCentric)
                {
                    newCelestials.Add(celestial);
                    continue;
                }

                var newPosition = celestial.Item2 + celestial.Item1.Velocity;

                var orbitalRadius = (celestial.Item1.Apogee + celestial.Item1.Perigree) / 2;
                float fullOrbitDistance = (float)Math.PI * (orbitalRadius ^ 2);

                //There are 
                if(newPosition > fullOrbitDistance)
                    newPosition = fullOrbitDistance - newPosition;

                newCelestials.Add(new Tuple<ICelestial, float>(celestial.Item1, newPosition));
            }

            CelestialPositions = newCelestials;
            Save();

            return true;
        }
    }
}
