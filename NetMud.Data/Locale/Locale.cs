using NetMud.Cartography;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Zone;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Locale
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
        public override string TemplateName
        {
            get
            {
                return Template<ILocaleTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(ILocaleTemplate), TemplateId));
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
                return "Locale_" + TemplateName;
            }
        }

        /// <summary>
        /// Is this zone discoverable?
        /// </summary>
        [Display(Name = "Always Discovered", Description = "Is this locale automatically known to players?")]
        [UIHint("Boolean")]
        public bool AlwaysDiscovered { get; set; }

        /// <summary>
        /// The interior map of the locale
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public ILiveMap Interior { get; set; }

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
                {
                    _parentLocation = new LiveCacheKey(value);
                }
            }
        }


        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Locale()
        {
            Descriptives = new HashSet<ISensoryEvent>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Locale(ILocaleTemplate locale)
        {
            TemplateId = locale.Id;

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
            //TODO
            return AlwaysDiscovered; // || discoverer.HasAccomplishment(DiscoveryName);
        }

        /// <summary>
        /// The center room of the specific zindex plane. TODO: Not sure if this should be a thing
        /// </summary>
        /// <param name="zIndex">The Z plane to find the central room for</param>
        /// <returns>The central room</returns>
        public IRoom CentralRoom(int zIndex = -1)
        {
            if (Interior == null)
                return Rooms().FirstOrDefault();

            return Cartographer.FindCenterOfMap(Interior.CoordinatePlane, zIndex);
        }

        /// <summary>
        /// How big (on average) this is in all 3 dimensions
        /// </summary>
        /// <returns>dimensional size</returns>
        public Dimensions Diameter()
        {
            //TODO
            return new Dimensions(1, 1, 1);
        }

        /// <summary>
        /// Absolute max dimensions in each direction
        /// </summary>
        /// <returns>absolute max dimensional size</returns>
        public Dimensions FullDimensions()
        {
            //TODO
            return new Dimensions(1, 1, 1);
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
            ILocaleTemplate bS = Template<ILocaleTemplate>() ?? throw new InvalidOperationException("Missing backing data store on locale spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };
            AlwaysDiscovered = bS.AlwaysDiscovered;
            Descriptives = bS.Descriptives;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);

            ParentLocation = LiveCache.Get<IZone>(bS.ParentLocation.Id);

            if (spawnTo?.CurrentZone == null)
            {
                spawnTo = new GlobalPosition(ParentLocation, this);
            }

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            ILocale me = LiveCache.Get<ILocale>(TemplateId, typeof(LocaleTemplate));

            //Isn't in the world currently
            if (me == default(ILocale))
            {
                SpawnNewInWorld();
            }
        }

        /// <summary>
        /// Gets the model dimensions, actually a passthru to FullDimensions
        /// </summary>
        /// <returns></returns>
        public override Dimensions GetModelDimensions()
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
            return Rendering.RenderRadiusMap(this, 10, zIndex, forAdmin).Item2;
        }

        /// <summary>
        /// Regenerate the internal map for the locale; try not to do this often
        /// </summary>
        public void RemapInterior()
        {
            string[,,] returnMap = Cartographer.GenerateMapFromRoom(CentralRoom(), new HashSet<IRoom>(Rooms()), true);

            Interior = new LiveMap(returnMap, false);
        }

        /// <summary>
        /// Get adjascent surrounding locales and zones
        /// </summary>
        /// <returns>The adjascent locales and zones</returns>
        public IEnumerable<ILocation> GetSurroundings()
        {
            List<ILocation> radiusLocations = new List<ILocation>();
            IEnumerable<IPathway> paths = LiveCache.GetAll<IPathway>().Where(path => path.Origin.Equals(this));

            //If we don't have any paths out what can we even do
            if (paths.Count() == 0)
            {
                return radiusLocations;
            }

            while (paths.Count() > 0)
            {
                IEnumerable<ILocation> currentLocsSet = paths.Select(path => path.Destination);

                if (currentLocsSet.Count() == 0)
                {
                    break;
                }

                radiusLocations.AddRange(currentLocsSet);
                paths = currentLocsSet.SelectMany(ro => ro.GetPathways());
            }

            return radiusLocations;
        }

        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override ILexicalParagraph RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Add the existential modifiers
            return new LexicalParagraph(GetImmediateDescription(viewer, sensoryTypes[0]));
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public ILocale GetLiveInstance()
        {
            return this;
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
