using NetMud.Data.EntityBackingData;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Existential;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Gaia.Geographical;
using NetMud.Gaia.Meteorological;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Game
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LocationEntityPartial, IZone
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
                return DataTemplate<IZoneData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T DataTemplate<T>()
        {
            return (T)BackingDataCache.Get(new BackingDataCacheKey(typeof(IZoneData), DataTemplateId));
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
                return "Zone_" + DataTemplateName;
            }
        }

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        public bool Claimable { get; set; }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();

            Claimable = false;
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Zone(IZoneData zone)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();

            Claimable = false;

            DataTemplateId = zone.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Get the live world associated with this live zone
        /// </summary>
        /// <returns>The world</returns>
        public IGaia GetWorld()
        {
            var gaiaData = DataTemplate<IZoneData>().World;

            if (gaiaData != null)
                return gaiaData.GetLiveInstance();

            return null;
        }

        /// <summary>
        /// Generate a new random locale with rooms, put it in the world (and this zone) as temporary
        /// </summary>
        /// <param name="name">The name of the new locale, empty = generate a new one</param>
        /// <returns></returns>
        public ILocale GenerateAdventure(string name = "")
        {
            //TODO
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get all possible surroundings (locales, rooms, etc)
        /// </summary>
        /// <param name="strength">How far to look, IGNORED</param>
        /// <returns>All locations that are visible</returns>
        public override IEnumerable<ILocation> GetSurroundings(int strength)
        {
            //Zone is always 1
            return base.GetSurroundings(1);
        }
        
        /// <summary>
        /// Does this entity know about this thing
        /// </summary>
        /// <param name="discoverer">The onlooker</param>
        /// <returns>If this is known to the discoverer</returns>
        public bool IsDiscovered(IEntity discoverer)
        {
            if (DataTemplate<IZoneData>().AlwaysDiscovered)
                return true;

            //TODO

            //discoverer.HasQuality(DiscoveryName);

            //For now
            return true;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> GetShortDescription(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            return new List<string>()
            {
                String.Format("A {0} and {1} {2} {3} area.", GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType()),
                                                            MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()),
                                                            MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()),
                                                            GetBiome())
            };
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override IEnumerable<string> GetLongDescription(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
                return Enumerable.Empty<string>();

            var sb = new List<string>();

            //Weather/biome
            sb.Add(String.Format("A {0} and {1} {2} {3} area.", GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType()),
                                                            MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()),
                                                            MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()),
                                                            GetBiome()));

            if (NaturalResources != null)
                sb.AddRange(NaturalResources.Select(kvp => kvp.Key.RenderResourceCollection(viewer, kvp.Value)));

            //sb.AddRange(GetPathways().SelectMany(path => path.RenderAsContents(viewer)));
            //sb.AddRange(GetContents<IInanimate>().SelectMany(path => path.RenderAsContents(viewer)));
            //sb.AddRange(GetContents<IMobile>().Where(player => !player.Equals(viewer)).SelectMany(path => path.RenderAsContents(viewer)));

            return sb;
        }


        /// <summary>
        /// Get the h,w,d of this
        /// </summary>
        /// <returns></returns>
        public override Tuple<int, int, int> GetModelDimensions()
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

            return DataTemplate<IZoneData>().VisualAcuity;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IActor viewer)
        {
            var zD = DataTemplate<IZoneData>();
            var canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            var returnList = new List<ICelestial>();

            if (!canSeeSky)
                return returnList;

            var world = GetWorld();
            var celestials = world.CelestialPositions;

            if (celestials.Count() > 0)
            {
                //TODO: Add cloud cover stuff
                var rotationalPosition = world.PlanetaryRotation;
                var orbitalPosition = world.OrbitalPosition;
                var brightness = GetCurrentLuminosity();

                foreach(var celestial in celestials)
                {
                    var celestialLumins = celestial.Item1.Luminosity * AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.Item1, celestial.Item2, rotationalPosition, orbitalPosition
                                                                                                   , zD.Hemisphere, world.DataTemplate<IGaiaData>().RotationalAngle);

                    //Modify the brightness of the thing by the person looking at it
                    celestialLumins *= viewer.GetVisionModifier(brightness);

                    //how washed out is this thing compared to how bright the room is
                    if (celestialLumins >= brightness)
                        returnList.Add(celestial.Item1);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            var zD = DataTemplate<IZoneData>();
            float lumins = 0;
            var canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            //TODO: Add cloud cover
            if (canSeeSky)
            {
                var world = GetWorld();
                var celestials = world.CelestialPositions;
                var rotationalPosition = world.PlanetaryRotation;
                var orbitalPosition = world.OrbitalPosition;

                foreach (var celestial in celestials)
                {
                    var celestialAffectModifier = AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.Item1, celestial.Item2, rotationalPosition, orbitalPosition
                                                                                                        , zD.Hemisphere, world.DataTemplate<IGaiaData>().RotationalAngle);

                    lumins += celestial.Item1.Luminosity * celestialAffectModifier;
                }
            }

            foreach (var dude in MobilesInside.EntitiesContained())
                lumins += dude.GetCurrentLuminosity();

            lumins += Contents.EntitiesContained().Sum(c => c.GetCurrentLuminosity());
            lumins += MobilesInside.EntitiesContained().Sum(c => c.GetCurrentLuminosity());
            lumins += LiveCache.GetAll<ILocale>().Where(loc => loc.ParentLocation.Equals(this)).Sum(c => c.GetCurrentLuminosity());

            return lumins;
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        /// <param name="spawnTo">Where this will go</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            var bS = DataTemplate<IZoneData>() ?? throw new InvalidOperationException("Missing backing data store on zone spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };

            if(NaturalResources == null)
                NaturalResources = new Dictionary<INaturalResource, int>();

            if (String.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            if (spawnTo?.CurrentLocation == null)
                spawnTo = new GlobalPosition(this);

                CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            var me = LiveCache.Get<IZone>(DataTemplateId, typeof(ZoneData));

            //Isn't in the world currently
            if (me == default(IZone))
                SpawnNewInWorld();
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                DataTemplateId = me.DataTemplateId;
                Contents = me.Contents;
                MobilesInside = me.MobilesInside;
                Keywords = me.Keywords;
                CurrentLocation = new GlobalPosition(this);
                NaturalResources = me.NaturalResources;
            }
        }

        public IZone GetLiveInstance()
        {
            return this;
        }
    }
}
