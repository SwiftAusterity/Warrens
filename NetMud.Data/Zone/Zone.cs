using NetMud.CentralControl;
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
using NetMud.DataStructure.NaturalResource;
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
            FloraNaturalResources = new HashSet<INaturalResourceSpawn<IFlora>>();
            FaunaNaturalResources = new HashSet<INaturalResourceSpawn<IFauna>>();
            MineralNaturalResources = new HashSet<INaturalResourceSpawn<IMineral>>();
            Descriptives = new HashSet<ISensoryEvent>();
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
            FloraNaturalResources = new HashSet<INaturalResourceSpawn<IFlora>>();
            FaunaNaturalResources = new HashSet<INaturalResourceSpawn<IFauna>>();
            MineralNaturalResources = new HashSet<INaturalResourceSpawn<IMineral>>();
            Descriptives = new HashSet<ISensoryEvent>();

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
            Message mc = new Message
            {
                ToOrigin = new List<ILexicalParagraph>() { new LexicalParagraph(message) }
            };

            BroadcastEvent(mc, sender, subject, target);
        }

        private void BroadcastEvent(IMessage message, IEntity sender = null, IEntity subject = null, IEntity target = null)
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
        /// Gets the locales in this zone
        /// </summary>
        /// <returns>Locales</returns>
        public IEnumerable<ILocale> GetLocales()
        {
            return LiveCache.GetAll<ILocale>().Where(locale => locale.ParentLocation == this);
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
        public override ILexicalParagraph GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            var collectiveContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.None,
                Tense = LexicalTense.Present
            };

            var discreteContext = new LexicalContext(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Far,
                Tense = LexicalTense.Present
            };

            //Self becomes the first sense in the list
            List<ISensoryEvent> sensoryOutput = new List<ISensoryEvent>();
            foreach (MessagingType sense in sensoryTypes)
            {
                var me = GetSelf(sense);
                switch (sense)
                {
                    case MessagingType.Audible:
                        me.Strength = GetAudibleDelta(viewer);

                        IEnumerable<ISensoryEvent> aDescs = GetAudibleDescriptives(viewer);

                        if (aDescs.Count() == 0)
                        {
                            continue;
                        }

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSounds = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", collectiveContext);
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSounds.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any())
                        {
                            me.TryModify(uberSounds);
                        }

                        break;
                    case MessagingType.Olefactory:
                        me.Strength = GetOlefactoryDelta(viewer);

                        IEnumerable<ISensoryEvent> oDescs = GetOlefactoryDescriptives(viewer);

                        if (oDescs.Count() == 0)
                        {
                            continue;
                        }

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSmells = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", collectiveContext);
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any())
                        {
                            me.TryModify(uberSmells);
                        }

                        break;
                    case MessagingType.Psychic:
                        me.Strength = GetPsychicDelta(viewer);

                        IEnumerable<ISensoryEvent> pDescs = GetPsychicDescriptives(viewer);

                        if (pDescs.Count() == 0)
                        {
                            continue;
                        }

                        me.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectivePsy = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", collectiveContext);

                        ILexica uberPsy = collectivePsy.TryModify(LexicalType.Verb, GrammaticalType.Verb, "sense");
                        uberPsy.TryModify(pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in pDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any())
                        {
                            me.TryModify(collectivePsy);
                        }

                        break;
                    case MessagingType.Taste:
                        continue;
                    case MessagingType.Tactile:
                        me.Strength = GetTactileDelta(viewer);

                        //Add the temperature
                        me.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Lexica[] {
                            new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString(), collectiveContext),
                            new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString(), collectiveContext)
                        });

                        break;
                    case MessagingType.Visible:
                        me.Strength = GetVisibleDelta(viewer);

                        IEnumerable<ISensoryEvent> vDescs = GetVisibleDescriptives(viewer);

                        if (vDescs.Count() > 0)
                        {
                            me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                            Lexica collectiveSight = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", collectiveContext);

                            ILexica uberSight = collectiveSight.TryModify(LexicalType.Verb, GrammaticalType.Verb, "see");
                            uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                            foreach (ISensoryEvent desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                            {
                                Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                                newDesc.TryModify(desc.Event.Modifiers);

                                uberSight.TryModify(newDesc);
                            }

                            if (uberSight.Modifiers.Any())
                            {
                                me.TryModify(collectiveSight);
                            }
                        }

                        //Describe the size and population of this zone
                        DimensionalSizeDescription zoneSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, zoneSize.ToString());

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

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, crowdSize);

                        break;
                }

                if (me != null)
                {
                    sensoryOutput.Add(me);
                }
            }

            foreach (ICelestial celestial in GetVisibileCelestials(viewer))
            {
                sensoryOutput.AddRange(celestial.RenderAsContents(viewer, sensoryTypes).Events);
            }

            foreach (IWeatherEvent wEvent in WeatherEvents)
            {
                sensoryOutput.AddRange(wEvent.RenderAsContents(viewer, sensoryTypes).Events);
            }

            foreach (var resource in FloraNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            foreach (var resource in FaunaNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            foreach (var resource in MineralNaturalResources)
            {
                sensoryOutput.AddRange(resource.Resource.RenderResourceCollection(viewer, resource.RateFactor).Events);
            }

            //render our locales out
            foreach (ILocale locale in LiveCache.GetAll<ILocale>().Where(loc => loc.ParentLocation?.TemplateId == TemplateId))
            {
                sensoryOutput.AddRange(locale.RenderAsContents(viewer, sensoryTypes).Events);
            }

            //render our locales out
            foreach (var path in GetPathways())
            {
                sensoryOutput.AddRange(path.RenderAsContents(viewer, sensoryTypes).Events);
            }

            return new LexicalParagraph(sensoryOutput);
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
            bool canSeeSky = IsOutside(); //TODO: cloud cover

            List<ICelestial> returnList = new List<ICelestial>();

            if (!canSeeSky)
                return returnList;

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
            bool canSeeSky = IsOutside();

            //TODO: Add cloud cover. Commented out for testing purposes ATM
            if (canSeeSky)
            {
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
            }

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
            Descriptives = bS.Descriptives;

            WeatherEvents = Enumerable.Empty<IWeatherEvent>();
            FloraNaturalResources = new HashSet<INaturalResourceSpawn<IFlora>>();
            FaunaNaturalResources = new HashSet<INaturalResourceSpawn<IFauna>>();
            MineralNaturalResources = new HashSet<INaturalResourceSpawn<IMineral>>();

            PopulateMap();

            UpsertToLiveWorldCache(true);

            KickoffProcesses();

            CurrentLocation = new GlobalPosition(this, null, null);
            UpsertToLiveWorldCache(true);

            Save();
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
                Descriptives = me.Descriptives;

                Qualities = me.Qualities;
                PopulateMap();
                KickoffProcesses();
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
        public override void KickoffProcesses()
        {
            //Start decay eventing for this zone
            Processor.StartSubscriptionLoop("NaturalResourceGeneration", () => AdvanceResources(), 30 * 60, false);
            base.KickoffProcesses();
        }

        private bool AdvanceResources()
        {
            var rand = new Random();
            var bS = Template<IZoneTemplate>();

            if (FloraNaturalResources.Count() == 0 && bS.FloraResourceSpawn.Count() != 0)
            {
                foreach (INaturalResourceSpawn<IFlora> population in bS.FloraResourceSpawn)
                {
                    FloraNaturalResources.Add(new FloraResourceSpawn() { RateFactor = population.RateFactor, Resource = population.Resource });
                }
            }
            else
            {
                foreach (INaturalResourceSpawn<IFlora> population in FloraNaturalResources)
                {
                    var baseRate = bS.FloraResourceSpawn.FirstOrDefault(spawn => spawn.Resource.Equals(population.Resource));

                    if (baseRate == null || population.RateFactor > 100 * baseRate.RateFactor)
                    {
                        continue;
                    }

                    population.RateFactor = Math.Min(100 * baseRate.RateFactor, baseRate.RateFactor * rand.Next(1, 3) + population.RateFactor);
                }
            }

            if (MineralNaturalResources.Count() == 0 && bS.MineralResourceSpawn.Count() != 0)
            {
                foreach (INaturalResourceSpawn<IMineral> population in bS.MineralResourceSpawn)
                {
                    MineralNaturalResources.Add(new MineralResourceSpawn() { RateFactor = population.RateFactor, Resource = population.Resource });
                }
            }
            else
            {
                foreach (INaturalResourceSpawn<IMineral> population in MineralNaturalResources)
                {
                    var baseRate = bS.MineralResourceSpawn.FirstOrDefault(spawn => spawn.Resource.Equals(population.Resource));

                    if (baseRate == null || population.RateFactor > 25 * baseRate.RateFactor)
                    {
                        continue;
                    }

                    population.RateFactor = Math.Min(100 * baseRate.RateFactor, baseRate.RateFactor + population.RateFactor);
                }
            }

            if (FaunaNaturalResources.Count() == 0 && bS.FaunaResourceSpawn.Count() != 0)
            {
                foreach (INaturalResourceSpawn<IFauna> population in bS.FaunaResourceSpawn)
                {
                    FaunaNaturalResources.Add(new FaunaResourceSpawn() { RateFactor = population.RateFactor, Resource = population.Resource });
                }
            }
            else
            {
                foreach (INaturalResourceSpawn<IFauna> population in FaunaNaturalResources)
                {
                    var baseRate = bS.FaunaResourceSpawn.FirstOrDefault(spawn => spawn.Resource.Equals(population.Resource));

                    if (baseRate == null || population.RateFactor > 1000 * baseRate.RateFactor)
                    {
                        continue;
                    }

                    population.RateFactor = Math.Min(100 * baseRate.RateFactor, baseRate.RateFactor * population.Resource.FemaleRatio * 5 + population.RateFactor);
                }
            }

            Save();

            return true;
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
