using NetMud.Communication.Messaging;
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
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
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
        public override IOccurrence GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };

            //Self becomes the first sense in the list
            IOccurrence me = null;
            foreach (var sense in sensoryTypes)
            {
                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var aDescs = GetAudibleDescriptives(viewer);

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSounds = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSounds = collectiveSounds.TryModify(LexicalType.Verb, GrammaticalType.Verb, "hear");
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "distance")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "the");

                            uberSounds.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSounds);
                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var oDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSmells = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSmells = collectiveSmells.TryModify(LexicalType.Verb, GrammaticalType.Verb, "smell");
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "air")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "the");

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSmells);
                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var pDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectivePsy = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberPsy = collectivePsy.TryModify(LexicalType.Verb, GrammaticalType.Verb, "sense");
                        uberPsy.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "area")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "the");

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectivePsy);
                        break;
                    case MessagingType.Tactile:
                        continue;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                            continue;

                        if (me == null)
                            me = GetSelf(sense);

                        var vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        var collectiveSight = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        var uberSight = collectiveSight.TryModify(LexicalType.Verb, GrammaticalType.Verb, "see");
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (var desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            var newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "distance")
                                        .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "the");

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                            me.TryModify(collectiveSight);
                        break;
                }
            }

            //If we get through that and me is still null it means we can't detect anything at all
            if (me == null)
                return new Occurrence(sensoryTypes[0]);

            foreach (var celestial in GetVisibileCelestials(viewer))
                me.TryModify(celestial.RenderAsContents(viewer, sensoryTypes).Event);

            //TODO: different way of rendering natural resources
            if (NaturalResources != null)
                foreach (var resource in NaturalResources)
                    me.TryModify(resource.Key.RenderResourceCollection(viewer, resource.Value).Event);

            //render our locales out
            foreach (var locale in LiveCache.GetAll<ILocale>().Where(loc => loc.CurrentLocation.GetZone().Equals(this)))
                me.TryModify(locale.GetFullDescription(viewer, sensoryTypes));

            //Describe the size and population of this zone
            var zoneSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

            var area = new Lexica(LexicalType.Noun, GrammaticalType.Subject, "space");
            area.TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "this");
            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, zoneSize.ToString());

            //Add the temperature
            area.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Lexica[] {
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString()),
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString())
            });

            //Render people in the zone
            var populationSize = GeographicalUtilities.GetCrowdSize(GetContents<IMobile>().Count());

            var crowdSize = "abandoned";
            if ((short)populationSize > (short)zoneSize)
                crowdSize = "crowded";
            else if (populationSize > CrowdSizeDescription.Intimate)
                crowdSize = "sparsely populated";

            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, crowdSize);

            me.TryModify(area);

            return me;
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
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            var zD = DataTemplate<IZoneData>();
            var canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            var returnList = new List<ICelestial>();

            //if (!canSeeSky)
            //  return returnList;

            var world = GetWorld();
            var celestials = world.CelestialPositions;

            if (celestials.Count() > 0)
            {
                //TODO: Add cloud cover stuff
                var rotationalPosition = world.PlanetaryRotation;
                var orbitalPosition = world.OrbitalPosition;
                var currentBrightness = CurrentLocation.CurrentLocation.GetCurrentLuminosity();

                foreach (var celestial in celestials)
                {
                    var celestialLumins = celestial.Item1.Luminosity * AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.Item1, celestial.Item2, rotationalPosition, orbitalPosition
                                                                                                   , zD.Hemisphere, world.DataTemplate<IGaiaData>().RotationalAngle);

                    //how washed out is this thing compared to how bright the room is
                    if (celestialLumins >= currentBrightness / 10)
                        returnList.Add(celestial.Item1);
                }
            }

            return returnList;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public override IEnumerable<IOccurrence> GetVisibleDescriptives(IEntity viewer)
        {
            var descriptives = base.GetVisibleDescriptives(viewer).ToList();
            var currentLumins = GetCurrentLuminosity();
            var viewerRange = viewer.GetVisualRange();

            descriptives.AddRange(DataTemplate<IZoneData>().Descriptives);
            descriptives.AddRange(GetVisibileCelestials(viewer).SelectMany(c => c.Descriptives));

            return descriptives.Where(d => currentLumins >= viewerRange.Item1 / d.Strength && currentLumins <= d.Strength * viewerRange.Item2);
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

            //TODO: Add cloud cover. Commented out for testing purposes ATM
            //if (canSeeSky)
            //{
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
            //}

            lumins += Contents.EntitiesContained().Sum(c => c.GetCurrentLuminosity());
            lumins += MobilesInside.EntitiesContained().Sum(c => c.GetCurrentLuminosity());

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

            if (NaturalResources == null)
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
