using NetMud.Cartography;
using NetMud.Data.DataIntegrity;
using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Live locale (collection of rooms in a zone)
    /// </summary>
    public class Locale : EntityPartial, ILocale
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
                return DataTemplate<ILocaleData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(ILocaleData), DataTemplateId));
        }

        /// <summary>
        /// The name used in the tag for discovery checking
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public string DiscoveryName
        {
            get
            {
                return "Locale_" + DataTemplateName;
            }
        }

        /// <summary>
        /// The interior map of the locale
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public IMap Interior { get; set; }

        [JsonProperty("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The zone this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Locales must have a zone affiliation.")]
        public IZone ParentLocation
        {
            get
            {
                return LiveCache.Get<IZone>(_parentLocation);
            }
            set
            {
                if (value != null)
                    _parentLocation = new LiveCacheKey(value);
            }
        }


        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Locale()
        {
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Locale(ILocaleData locale)
        {
            DataTemplateId = locale.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Live rooms in this locale
        /// </summary>
        public IEnumerable<IRoom> Rooms()
        {
            return LiveCache.GetAll<IRoom>().Where(room => room.ParentLocation.Equals(this));
        }

        /// <summary>
        /// Does this entity know about this thing
        /// </summary>
        /// <param name="discoverer">The onlooker</param>
        /// <returns>If this is known to the discoverer</returns>
        public bool IsDiscovered(IEntity discoverer)
        {
            if (DataTemplate<ILocaleData>().AlwaysDiscovered)
                return true;

            //TODO

            //discoverer.HasAccomplishment(DiscoveryName);

            //For now
            return true;
        }

        /// <summary>
        /// The center room of the specific zindex plane. TODO: Not sure if this should be a thing
        /// </summary>
        /// <param name="zIndex">The Z plane to find the central room for</param>
        /// <returns>The central room</returns>
        public IRoom CentralRoom(int zIndex = -1)
        {
            return (IRoom)Cartographer.FindCenterOfMap(DataTemplate<ILocaleData>().Interior.CoordinatePlane, zIndex).GetLiveInstance();
        }

        /// <summary>
        /// How big (on average) this is in all 3 dimensions
        /// </summary>
        /// <returns>dimensional size</returns>
        public Tuple<int, int, int> Diameter()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        /// <summary>
        /// Absolute max dimensions in each direction
        /// </summary>
        /// <returns>absolute max dimensional size</returns>
        public Tuple<int, int, int> FullDimensions()
        {
            //TODO
            return new Tuple<int, int, int>(1, 1, 1);
        }

        /// <summary>
        /// Gets the actual vision modifier taking into account blindness and other factors
        /// </summary>
        /// <returns>the working modifier</returns>
        public override float GetVisionModifier(float currentBrightness)
        {
            //Base case doesn't count "lumin vision range" mobiles/players have, inanimate entities are assumed to have unlimited light and dark vision

            //TODO: Check for blindess/magical type affects

            return DataTemplate<ILocaleData>().VisualAcuity;
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            return Rooms().Sum(r => r.GetCurrentLuminosity());
        }

        /// <summary>
        /// Render the locale to a specific look
        /// </summary>
        /// <param name="actor">Who is looking</param>
        /// <returns>The locale's description</returns>
        public override IEnumerable<string> RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            return Enumerable.Empty<string>();
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(ParentLocation));
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<ILocaleData>() ?? throw new InvalidOperationException("Missing backing data store on locale spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            ParentLocation = (IZone)bS.ParentLocation.GetLiveInstance();

            if (spawnTo?.CurrentLocation == null)
                spawnTo = new GlobalPosition(ParentLocation);

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<ILocale>(DataTemplateId, typeof(LocaleData));

            //Isn't in the world currently
            if (me == default(ILocale))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Keywords = me.Keywords;
                CurrentLocation = me.CurrentLocation;
                ParentLocation = me.ParentLocation;
            }
        }

        /// <summary>
        /// Gets the model dimensions, actually a passthru to FullDimensions
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
        {
            return FullDimensions();
        }

        /// <summary>
        /// Renders the map
        /// </summary>
        /// <param name="zIndex">the Z plane to render flat</param>
        /// <param name="forAdmin">Is this visibility agnostic</param>
        /// <returns>The rendered flat map</returns>
        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return DataTemplate<ILocaleData>().RenderMap(zIndex, forAdmin);
        }

        /// <summary>
        /// Get adjascent surrounding locales and zones
        /// </summary>
        /// <returns>The adjascent locales and zones</returns>
        public IEnumerable<ILocation> GetSurroundings()
        {
            var radiusLocations = new List<ILocation>();
            var paths = LiveCache.GetAll<IPathway>().Where(path => path.Origin.Equals(this));

            //If we don't have any paths out what can we even do
            if (paths.Count() == 0)
                return radiusLocations;

            while (paths.Count() > 0)
            {
                var currentLocsSet = paths.Select(path => path.Destination);

                if (currentLocsSet.Count() == 0)
                    break;

                radiusLocations.AddRange(currentLocsSet);
                paths = currentLocsSet.SelectMany(ro => ro.GetPathways());
            }

            return radiusLocations;
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public ILocale GetLiveInstance()
        {
            return this;
        }
    }
}
