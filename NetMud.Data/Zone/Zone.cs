using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Gaia.Geographical;
using NetMud.Gaia.Meteorological;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Zone
{
    /// <summary>
    /// Zones contain rooms
    /// </summary>
    [Serializable]
    public class Zone : LocationEntityPartial, IZone
    {
        #region Template and Framework Values
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<IZoneTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IZoneTemplate), TemplateId));
        }

        /// <summary>
        /// Paths out of this zone
        /// </summary>
        public HashSet<IPathway> Pathways { get; set; }
        #endregion

        /// <summary>
        /// What hemisphere this zone is in
        /// </summary>
        [Display(Name = "Hemisphere", Description = "The hemisphere of the world this zone is in.")]
        [UIHint("EnumDropDownList")]
        public HemispherePlacement Hemisphere { get; set; }

        /// <summary>
        /// Base elevation used in generating locales
        /// </summary>
        [Display(Name = "Base Elevation", Description = "What the central elevation is.")]
        [DataType(DataType.Text)]
        public int BaseElevation { get; set; }

        [JsonIgnore]
        [ScriptIgnore]
        public IMap _map { get; set; }

        public IMap Map
        {
            get
            {
                if (_map == null)
                {
                    PopulateMap();
                }

                return _map;
            }
            private set
            {
                _map = value;
            }
        }

        /// <summary>
        /// Clouds, basically
        /// </summary>
        public IEnumerable<IWeatherEvent> WeatherEvents { get; set; }

        /// <summary>
        /// New up a "blank" zone entry
        /// </summary>
        public Zone()
        {
            Qualities = new HashSet<IQuality>();
            WeatherEvents = Enumerable.Empty<IWeatherEvent>();
            MobilesInside = new EntityContainer<IMobile>();
            Contents = new EntityContainer<IInanimate>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Zone(IZoneTemplate zone)
        {
            TemplateId = zone.Id;
            Qualities = new HashSet<IQuality>();
            WeatherEvents = Enumerable.Empty<IWeatherEvent>();
            MobilesInside = new EntityContainer<IMobile>();
            Contents = new EntityContainer<IInanimate>();

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Broadcast an event to the entire zone
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="sender">the sender</param>
        /// <param name="subject">the subject</param>
        /// <param name="target">the target</param>
        public void BroadcastEvent(string message, IEntity sender = null, IEntity subject = null, IEntity target = null)
        {
            MessageCluster mc = new MessageCluster
            {
                ToOrigin = new List<IMessage>() { new Message(message) }
            };

            BroadcastEvent(mc, sender, subject, target);
        }

        private void BroadcastEvent(IMessageCluster message, IEntity sender = null, IEntity subject = null, IEntity target = null)
        {
            message.ExecuteMessaging(sender, subject, target, this, null);
        }

        /// <summary>
        /// Get the live world associated with this live zone
        /// </summary>
        /// <returns>The world</returns>
        public IGaia GetWorld()
        {
            IGaiaTemplate GaiaTemplate = Template<IZoneTemplate>().World;

            if (GaiaTemplate != null)
            {
                return GaiaTemplate.GetLiveInstance();
            }

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
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override ISensoryEvent GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Self becomes the first sense in the list
            ISensoryEvent me = null;
            foreach (MessagingType sense in sensoryTypes)
            {
                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> aDescs = GetAudibleDescriptives(viewer);

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectiveSounds = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberSounds = collectiveSounds.TryModify(LexicalType.Verb, GrammaticalType.Verb, "hear");
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "distance")
                                        .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "the");

                            uberSounds.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectiveSounds);
                        }

                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> oDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectiveSmells = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberSmells = collectiveSmells.TryModify(LexicalType.Verb, GrammaticalType.Verb, "smell");
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "air")
                                        .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "the");

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectiveSmells);
                        }

                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> pDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectivePsy = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberPsy = collectivePsy.TryModify(LexicalType.Verb, GrammaticalType.Verb, "sense");
                        uberPsy.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "area")
                                        .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "the");

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectivePsy);
                        }

                        break;
                    case MessagingType.Taste:
                    case MessagingType.Tactile:
                        continue;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectiveSight = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberSight = collectiveSight.TryModify(LexicalType.Verb, GrammaticalType.Verb, "see");
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            newDesc.TryModify(LexicalType.Noun, GrammaticalType.IndirectObject, "distance")
                                        .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "in")
                                            .TryModify(LexicalType.Article, GrammaticalType.Descriptive, "the");

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectiveSight);
                        }

                        break;
                }
            }

            //If we get through that and me is still null it means we can't detect anything at all
            if (me == null)
            {
                return new SensoryEvent(sensoryTypes[0]);
            }

            foreach (ICelestial celestial in GetVisibileCelestials(viewer))
            {
                me.TryModify(celestial.RenderAsContents(viewer, sensoryTypes).Event);
            }

            //TODO: different way of rendering natural resources
            if (NaturalResources != null)
            {
                foreach (KeyValuePair<DataStructure.NaturalResource.INaturalResource, int> resource in NaturalResources)
                {
                    me.TryModify(resource.Key.RenderResourceCollection(viewer, resource.Value).Event);
                }
            }

            //render our locales out
            foreach (ILocale locale in LiveCache.GetAll<ILocale>().Where(loc => loc.ParentLocation?.TemplateId == TemplateId))
            {
                me.TryModify(locale.GetFullDescription(viewer, sensoryTypes));
            }

            //Describe the size and population of this zone
            DimensionalSizeDescription zoneSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

            Lexica area = new Lexica(LexicalType.Noun, GrammaticalType.Subject, "space");
            area.TryModify(LexicalType.Article, GrammaticalType.Descriptive, "this");
            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, zoneSize.ToString());

            //Add the temperature
            area.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Lexica[] {
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString()),
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString())
            });

            //Render people in the zone
            CrowdSizeDescription populationSize = GeographicalUtilities.GetCrowdSize(GetContents<IMobile>().Count());

            string crowdSize = "abandoned";
            if ((short)populationSize > (short)zoneSize)
            {
                crowdSize = "crowded";
            }
            else if (populationSize > CrowdSizeDescription.Intimate)
            {
                crowdSize = "sparsely populated";
            }

            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, crowdSize);

            me.TryModify(area);

            return me;
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
        /// Get the h,w,d of this
        /// </summary>
        /// <returns></returns>
        public override Dimensions GetModelDimensions()
        {
            //TODO
            return new Dimensions(1, 1, 1);
        }

        /// <summary>
        /// Does this entity know about this thing
        /// </summary>
        /// <param name="discoverer">The onlooker</param>
        /// <returns>If this is known to the discoverer</returns>
        public bool IsDiscovered(IEntity discoverer)
        {
            if (Template<IZoneTemplate>().AlwaysDiscovered)
            {
                return true;
            }

            //TODO

            //discoverer.HasQuality(DiscoveryName);

            //For now
            return true;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            IZoneTemplate zD = Template<IZoneTemplate>();
            bool canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            List<ICelestial> returnList = new List<ICelestial>();

            //if (!canSeeSky)
            //  return returnList;

            IGaia world = GetWorld();
            IEnumerable<ICelestialPosition> celestials = world.CelestialPositions;

            if (celestials.Count() > 0)
            {
                //TODO: Add cloud cover stuff
                float rotationalPosition = world.PlanetaryRotation;
                float orbitalPosition = world.OrbitalPosition;
                float currentBrightness = GetCurrentLuminosity();

                foreach (ICelestialPosition celestial in celestials)
                {
                    float celestialLumins = celestial.CelestialObject.Luminosity * AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.CelestialObject, celestial.Position, rotationalPosition, orbitalPosition
                                                                                                   , zD.Hemisphere, world.Template<IGaiaTemplate>().RotationalAngle);

                    //how washed out is this thing compared to how bright the room is
                    if (celestialLumins / currentBrightness > 0.01)
                    {
                        returnList.Add(celestial.CelestialObject);
                    }
                }
            }

            return returnList;
        }

        /// <summary>
        /// Get the current forecast for this zone
        /// </summary>
        /// <returns>Bunch of stuff</returns>
        public Tuple<PrecipitationAmount, PrecipitationType, HashSet<WeatherType>> CurrentForecast()
        {
            PrecipitationAmount pAmount = PrecipitationAmount.Clear;
            PrecipitationType pType = PrecipitationType.Clear;
            HashSet<WeatherType> wTypes = new HashSet<WeatherType>();

            float totalRainVolume = WeatherEvents.Where(wev => wev.Type != WeatherEventType.Tectonic).Sum(wev => wev.PrecipitationAmount);
            float totalStrength = WeatherEvents.Where(wev => wev.Type != WeatherEventType.Tectonic).Sum(wev => wev.Strength);

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Tectonic))
            {
                wTypes.Add(WeatherType.Earthquake);
            }

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Cyclone))
            {
                wTypes.Add(WeatherType.Tornado);
            }

            if (WeatherEvents.Any(wev => wev.Type == WeatherEventType.Typhoon))
            {
                wTypes.Add(WeatherType.Hurricane);
            }

            if (!wTypes.Contains(WeatherType.Tornado) && !wTypes.Contains(WeatherType.Hurricane))
            {
                if (totalStrength > 50)
                {
                    wTypes.Add(WeatherType.Storming);
                }
                else if (totalStrength > 10)
                {
                    wTypes.Add(WeatherType.Windy);
                }
            }

            if (totalRainVolume > 0)
            {
                wTypes.Add(WeatherType.Precipitation);

                if (totalRainVolume < 25)
                {
                    pAmount = PrecipitationAmount.Drizzle;
                }
                else if (totalRainVolume < 50)
                {
                    pAmount = PrecipitationAmount.Steady;
                }
                else if (totalRainVolume < 75)
                {
                    pAmount = PrecipitationAmount.Downpour;
                }
                else
                {
                    pAmount = PrecipitationAmount.Torrential;
                }

                int effTemp = EffectiveTemperature();

                if (effTemp > 5)
                {
                    pType = PrecipitationType.Rain;
                }
                else if (effTemp > -5)
                {
                    pType = PrecipitationType.Snow;
                }
                else
                {
                    pType = PrecipitationType.Freezing;
                }

                if (pType == PrecipitationType.Snow)
                {
                    if (totalStrength <= 10)
                    {
                        pType = PrecipitationType.Sleet;
                    }
                    else if (totalStrength >= 50)
                    {
                        pType = PrecipitationType.Hail;
                    }
                }
            }

            if (pAmount == PrecipitationAmount.Clear && pType == PrecipitationType.Clear)
            {
                wTypes.Add(WeatherType.Clear);
            }

            return new Tuple<PrecipitationAmount, PrecipitationType, HashSet<WeatherType>>(pAmount, pType, wTypes);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            IZoneTemplate zD = Template<IZoneTemplate>();
            float lumins = 0;
            bool canSeeSky = GeographicalUtilities.IsOutside(GetBiome());

            //TODO: Add cloud cover. Commented out for testing purposes ATM
            //if (canSeeSky)
            //{
            IGaia world = GetWorld();
            if (world != null)
            {
                IEnumerable<ICelestialPosition> celestials = world.CelestialPositions;
                float rotationalPosition = world.PlanetaryRotation;
                float orbitalPosition = world.OrbitalPosition;

                foreach (ICelestialPosition celestial in celestials)
                {
                    float celestialAffectModifier = AstronomicalUtilities.GetCelestialLuminosityModifier(celestial.CelestialObject, celestial.Position, rotationalPosition, orbitalPosition
                                                                                                        , zD.Hemisphere, world.Template<IGaiaTemplate>().RotationalAngle);

                    lumins += celestial.CelestialObject.Luminosity * celestialAffectModifier;
                }
            }
            //}

            return lumins;
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this, null, null));
        }

        /// <summary>
        /// Spawn this into the world and live cache
        /// </summary>
        /// <param name="spawnTo">Where this will go</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IZoneTemplate bS = Template<IZoneTemplate>() ?? throw new InvalidOperationException("Missing backing data store on zone spawn event.");

            Keywords = bS.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            Qualities = bS.Qualities;

            WeatherEvents = Enumerable.Empty<IWeatherEvent>();

            PopulateMap();

            UpsertToLiveWorldCache(true);

            KickoffProcesses();

            CurrentLocation = new GlobalPosition(this, null, null);
            UpsertToLiveWorldCache(true);
        }

        /// <summary>
        /// Get this from the world or make a new one and put it in
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IZone me = LiveCache.Get<IZone>(TemplateId, typeof(ZoneTemplate));

            //Isn't in the world currently
            if (me == default(IZone))
            {
                SpawnNewInWorld();
            }
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = me.TemplateId;
                Keywords = me.Keywords;
                CurrentLocation = new GlobalPosition(this, null, null);

                Qualities = me.Qualities;
                PopulateMap();
            }
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            string error = string.Empty;

            if (CurrentLocation?.CurrentLocation() != null)
            {
                error = CurrentLocation.CurrentLocation().MoveFrom(this);
            }

            //validate position
            if (newPosition != null && string.IsNullOrEmpty(error))
            {
                if (newPosition.CurrentLocation() != null)
                {
                    error = newPosition.CurrentLocation().MoveInto(this);
                }

                if (string.IsNullOrEmpty(error))
                {
                    CurrentLocation = newPosition;
                    UpsertToLiveWorldCache();
                    error = string.Empty;
                }
            }
            else
            {
                error = "Cannot move to an invalid location";
            }

            return error;
        }

        public override IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(this);
        }


        public IZone GetLiveInstance()
        {
            return this;
        }

        private void PopulateMap()
        {
            IZoneTemplate dt = Template<IZoneTemplate>();
        }

        #region Processes
        internal override void KickoffProcesses()
        {
            //Start decay eventing for this zone
            base.KickoffProcesses();
        }
        #endregion

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Zone
            {
                Qualities = Qualities,
                TemplateId = TemplateId,
                Humidity = Humidity,
                Temperature = Temperature,
                Map = Map
            };
        }
    }
}
