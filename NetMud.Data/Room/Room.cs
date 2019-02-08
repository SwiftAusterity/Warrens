using NetMud.Communication.Lexical;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
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

namespace NetMud.Data.Room
{
    /// <summary>
    /// Places entities are (most of the time)
    /// </summary>
    [Serializable]
    public class Room : LocationEntityPartial, IRoom
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
                return Template<IRoomTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IRoomTemplate), TemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        [JsonProperty("ParentLocation")]
        private LiveCacheKey _parentLocation { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Rooms must have a locale affiliation.")]
        public ILocale ParentLocation
        {
            get
            {
                return LiveCache.Get<ILocale>(_parentLocation);
            }
            set
            {
                if (value != null)
                {
                    _parentLocation = new LiveCacheKey(value);
                }
            }
        }

        [ScriptIgnore]
        [JsonIgnore]
        private Coordinate _coordinates { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        public Coordinate Coordinates
        {
            get
            {
                return _coordinates;
            }
            set
            {
                _coordinates = value;

                var dt = Template<IRoomTemplate>();
                if (dt != null)
                {
                    dt.Coordinates = _coordinates;
                    dt.PersistToCache();
                }
                
            }
        }

        [JsonProperty("Medium")]
        private TemplateCacheKey _medium { get; set; }

        /// <summary>
        /// What is in the middle of the room
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("Medium material is invalid.")]
        [Display(Name = "Medium", Description = "What the 'empty' space of the room is made of. (likely AIR, sometimes stone or dirt)")]
        [DataType(DataType.Text)]
        public IMaterial Medium
        {
            get
            {
                return TemplateCache.Get<IMaterial>(_medium);
            }
            set
            {
                if (value != null)
                {
                    _medium = new TemplateCacheKey(value);
                }
            }
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Room()
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Coordinates = new Coordinate(-1, -1, -1);
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="room">the backing data</param>
        public Room(IRoomTemplate room)
        {
            Contents = new EntityContainer<IInanimate>();
            MobilesInside = new EntityContainer<IMobile>();
            Coordinates = new Coordinate(-1, -1, -1);

            TemplateId = room.Id;

            GetFromWorldOrSpawn();
        }

        /// <summary>
        /// Gets the remaining distance and next "step" to the destination room
        /// </summary>
        /// <param name="destination">The room you're heading for</param>
        /// <returns>distance (in rooms) and the next path you'd have to use</returns>
        public Tuple<int, IPathway> GetDistanceAndNextStepDestination(ILocation destination)
        {
            int distance = -1;
            IPathway nextStep = null;

            return new Tuple<int, IPathway>(distance, nextStep);
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            IRoomTemplate dT = Template<IRoomTemplate>();
            IZone zone = CurrentLocation.CurrentZone;

            bool canSeeSky = GeographicalUtilities.IsOutside(GetBiome()) 
                            && dT.Coordinates.Z >= zone.Template<IZoneTemplate>().BaseElevation;

            //if (!canSeeSky)
            //    return Enumerable.Empty<ICelestial>();

            //The zone knows about the celestial positioning
            return zone.GetVisibileCelestials(viewer);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            IZone zone = CurrentLocation.CurrentZone;
            if (zone != null)
            {
                zone.GetCurrentLuminosity();
            }

            foreach (IMobile dude in MobilesInside.EntitiesContained())
            {
                lumins += dude.GetCurrentLuminosity();
            }

            foreach (IInanimate thing in Contents.EntitiesContained())
            {
                lumins += thing.GetCurrentLuminosity();
            }

            return lumins;
        }

        public override IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(CurrentLocation.CurrentZone, CurrentLocation.CurrentLocale, this);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IRoom GetLiveInstance()
        {
            return this;
        }

        #region rendering
        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override ISensoryEvent RenderToLook(IEntity viewer)
        {
            return GetFullDescription(viewer);
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

                            uberPsy.TryModify(newDesc);
                        }

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectivePsy);
                        }

                        break;
                    case MessagingType.Taste:
                        if (!IsTastableTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> taDescs = GetPsychicDescriptives(viewer);

                        me.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectiveTaste = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberTaste = collectiveTaste.TryModify(LexicalType.Verb, GrammaticalType.Verb, "taste");
                        uberTaste.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTaste.TryModify(newDesc);
                        }

                        if (uberTaste.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectiveTaste);
                        }

                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                        {
                            continue;
                        }

                        if (me == null)
                        {
                            me = GetSelf(sense);
                        }

                        IEnumerable<ISensoryEvent> tDescs = GetTouchDescriptives(viewer);

                        me.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica collectiveTouch = new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you");

                        ILexica uberTouch = collectiveTouch.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feel");
                        uberTouch.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTouch.TryModify(newDesc);
                        }

                        if (uberTouch.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                        {
                            me.TryModify(collectiveTouch);
                        }

                        break;
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

            if (NaturalResources != null)
            {
                foreach (KeyValuePair<INaturalResource, int> resource in NaturalResources)
                {
                    me.TryModify(resource.Key.RenderResourceCollection(viewer, resource.Value).Event);
                }
            }

            foreach (IPathway path in GetPathways())
            {
                me.TryModify(path.RenderAsContents(viewer, sensoryTypes).Event);
            }

            foreach (IInanimate obj in GetContents<IInanimate>())
            {
                me.TryModify(obj.RenderAsContents(viewer, sensoryTypes).Event);
            }

            foreach (IMobile mob in GetContents<IMobile>().Where(player => !player.Equals(viewer)))
            {
                me.TryModify(mob.RenderAsContents(viewer, sensoryTypes).Event);
            }

            //Describe the size and population of this zone
            DimensionalSizeDescription roomSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

            Lexica area = new Lexica(LexicalType.Noun, GrammaticalType.Subject, "space");
            area.TryModify(LexicalType.Article, GrammaticalType.Descriptive, "this");
            area.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, roomSize.ToString());

            //Add the temperature
            area.TryModify(LexicalType.Verb, GrammaticalType.Verb, "feels").TryModify(new Lexica[] {
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertHumidityToType(EffectiveHumidity()).ToString()),
                new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, MeteorologicalUtilities.ConvertTemperatureToType(EffectiveTemperature()).ToString())
            });

            //Render people in the zone
            CrowdSizeDescription populationSize = GeographicalUtilities.GetCrowdSize(GetContents<IMobile>().Count());

            string crowdSize = "lonely";
            if ((short)populationSize > (short)roomSize)
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
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        public string RenderCenteredMap(int radius, bool visibleOnly)
        {
            //TODO: fix visibility
            return Cartography.Rendering.RenderRadiusMap(this, 3, visibleOnly);
        }
        #endregion

        #region Spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            IRoom me = LiveCache.Get<IRoom>(TemplateId, typeof(RoomTemplate));

            //Isn't in the world currently
            if (me == default(IRoom))
            {
                SpawnNewInWorld();
            }
            else
            {
                BirthMark = me.BirthMark;
                Birthdate = me.Birthdate;
                TemplateId = me.TemplateId;
                Contents = me.Contents;
                MobilesInside = me.MobilesInside;
                Keywords = me.Keywords;
                NaturalResources = me.NaturalResources;
                ParentLocation = me.ParentLocation;
                Model = me.Model;
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(new GlobalPosition(this));
        }


        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IRoomTemplate bS = Template<IRoomTemplate>() ?? throw new InvalidOperationException("Missing backing data store on room spawn event.");

            Keywords = new string[] { bS.Name.ToLower() };
            Model = bS.Model;

            if (NaturalResources == null)
            {
                NaturalResources = new Dictionary<INaturalResource, int>();
            }

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            UpsertToLiveWorldCache(true);

            ParentLocation = LiveCache.Get<ILocale>(bS.ParentLocation.Id);
            spawnTo.CurrentLocale = ParentLocation;
            spawnTo.CurrentZone = ParentLocation.ParentLocation;

            if (spawnTo?.CurrentLocale == null || spawnTo?.CurrentZone == null)
            {
                spawnTo = new GlobalPosition(this);
            }

            CurrentLocation = spawnTo;

            UpsertToLiveWorldCache(true);
        }

        public override string TryMoveTo(IGlobalPosition newPosition)
        {
            throw new NotImplementedException();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
