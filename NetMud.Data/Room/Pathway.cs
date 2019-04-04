using NetMud.Cartography;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Room
{
    /// <summary>
    /// Portals between locations
    /// </summary>
    [Serializable]
    public class Pathway : EntityPartial, IPathway
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
                return Template<IPathwayTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IPathwayTemplate), TemplateId));
        }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Movement messages trigger when moved through
        /// </summary>
        public IMessage Enter { get; set; }

        /// <summary>
        /// Cardinality direction this points towards
        /// </summary>
        public MovementDirectionType DirectionType { get; set; }

        /// <summary>
        /// Birthmark of live location this points into
        /// </summary>
        [JsonProperty("Destination")]
        private LiveCacheKey _currentDestinationBirthmark;

        /// <summary>
        /// Restful live location this points into
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("To Location must be valid.")]
        [Display(Name = "To Room", Description = "The room this leads to.")]
        [DataType(DataType.Text)]
        public ILocation Destination
        {
            get
            {
                if (_currentDestinationBirthmark != null)
                {
                    return (ILocation)LiveCache.Get(_currentDestinationBirthmark);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentDestinationBirthmark = new LiveCacheKey(value);
            }
        }

        /// <summary>
        /// Birthmark of live location this points out of
        /// </summary>
        [JsonProperty("Origin")]
        private LiveCacheKey _currentOriginBirthmark;

        /// <summary>
        /// Restful live location this points out of
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [NonNullableDataIntegrity("From Location must be valid.")]
        [Display(Name = "From Room", Description = "The room this originates from.")]
        [DataType(DataType.Text)]
        public ILocation Origin
        {
            get
            {
                if (_currentOriginBirthmark != null)
                {
                    return (ILocation)LiveCache.Get(_currentOriginBirthmark);
                }

                return null;
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _currentOriginBirthmark = new LiveCacheKey(value);
            }
        }

        [Range(-1, 360, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Degrees From North", Description = "The direction on a 360 plane. 360 and 0 are both directional north. 90 is east, 180 is south, 270 is west.")]
        [DataType(DataType.Text)]
        public int DegreesFromNorth { get; set; }

        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}. -1 is for non-cardinal exits.")]
        [Display(Name = "Incline Grade", Description = "-100 to 100 (negative being a decline) % grade of elevation change.")]
        [DataType(DataType.Text)]
        public int InclineGrade { get; set; }

        /// <summary>
        /// What type of path is this? (rooms, zones, locales, etc)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public PathwayType Type
        {
            get
            {
                if (Origin != null && Destination != null)
                {
                    bool originIsZone = Origin.GetType().GetInterfaces().Contains(typeof(IZone)) ? true : false;
                    bool destinationIsZone = Destination.GetType().GetInterfaces().Contains(typeof(IZone)) ? true : false;

                    if (originIsZone && destinationIsZone)
                    {
                        return PathwayType.Zones;
                    }

                    if (!originIsZone && !destinationIsZone)
                    {
                        if (((IRoom)Destination).ParentLocation.BirthMark != ((IRoom)Origin).ParentLocation.BirthMark)
                        {
                            return PathwayType.Locale;
                        }

                        return PathwayType.Rooms;
                    }

                    if (originIsZone)
                    {
                        return PathwayType.FromZone;
                    }

                    return PathwayType.ToZone;
                }

                return PathwayType.None;
            }
        }

        /// <summary>
        /// News up an empty entity
        /// </summary>
        public Pathway()
        {
            Enter = new Message();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Pathway(IPathwayTemplate backingStore)
        {
            Enter = new Message();
            TemplateId = backingStore.Id;
            DirectionType = Utilities.TranslateToDirection(backingStore.DegreesFromNorth, backingStore.InclineGrade);
            GetFromWorldOrSpawn();
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
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;

            //TODO: ???

            return lumins;
        }

        #region spawning
        /// <summary>
        /// Tries to find this entity in the world based on its Id or gets a new one from the db and puts it in the world
        /// </summary>
        public void GetFromWorldOrSpawn()
        {
            //Try to see if they are already there
            Pathway me = LiveCache.Get<Pathway>(TemplateId);

            //Isn't in the world currently
            if (me == default(Pathway))
            {
                SpawnNewInWorld();
            }
        }

        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            SpawnNewInWorld(null);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition position)
        {
            //We can't even try this until we know if the data is there
            IPathwayTemplate bS = Template<IPathwayTemplate>() ?? throw new InvalidOperationException("Missing backing data store on pathway spawn event.");

            Keywords = new string[] { bS.Name.ToLower(), DirectionType.ToString().ToLower() };

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            DegreesFromNorth = bS.DegreesFromNorth;
            InclineGrade = bS.InclineGrade;
            DirectionType = Utilities.TranslateToDirection(DegreesFromNorth, InclineGrade);
            Descriptives = bS.Descriptives;

            //paths need two locations
            if (bS.Origin.GetType() == typeof(RoomTemplate))
            {
                Origin = ((IRoomTemplate)bS.Origin).GetLiveInstance();
            }
            else
            {
                Origin = ((IZoneTemplate)bS.Origin).GetLiveInstance();
            }

            if (bS.Destination.GetType() == typeof(RoomTemplate))
            {
                Destination = ((IRoomTemplate)bS.Destination).GetLiveInstance();
            }
            else
            {
                Destination = ((IZoneTemplate)bS.Destination).GetLiveInstance();
            }


            CurrentLocation = (IGlobalPosition)Origin.CurrentLocation.Clone();
            Model = bS.Model;

            //Enter = new Message(new string[] { bS.MessageToActor }, new string[] { "$A$ enters you" }, new string[] { }, new string[] { bS.MessageToOrigin }, new string[] { bS.MessageToDestination });
            //Enter.ToSurrounding.Add(MessagingType.Visible, new Tuple<int, IEnumerable<string>>(bS.VisibleStrength, new string[] { bS.VisibleToSurroundings }));
            //Enter.ToSurrounding.Add(MessagingType.Audible, new Tuple<int, IEnumerable<string>>(bS.AudibleStrength, new string[] { bS.AudibleToSurroundings }));

            UpsertToLiveWorldCache(true);

            Save();
        }
        #endregion

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public override ILexicalParagraph RenderToVisible(IEntity viewer)
        {
            short strength = GetVisibleDelta(viewer);

            IPathwayTemplate bS = Template<IPathwayTemplate>();
            ISensoryEvent me = GetSelf(MessagingType.Visible, strength);

            if (bS.Descriptives.Any())
            {
                foreach (ISensoryEvent desc in bS.Descriptives)
                {
                    me.Event.TryModify(desc.Event);
                }
            }
            else
            {
                LexicalContext collectiveContext = new LexicalContext(viewer)
                {
                    Determinant = true,
                    Perspective = NarrativePerspective.SecondPerson,
                    Plural = false,
                    Position = LexicalPosition.Near,
                    Tense = LexicalTense.Present
                };

                LexicalContext discreteContext = new LexicalContext(viewer)
                {
                    Determinant = true,
                    Perspective = NarrativePerspective.ThirdPerson,
                    Plural = false,
                    Position = LexicalPosition.Attached,
                    Tense = LexicalTense.Present
                };

                Lexica verb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "leads", collectiveContext);

                //Fallback to using names
                if (DirectionType == MovementDirectionType.None)
                {
                    Lexica origin = new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, Origin.TemplateName, discreteContext);
                    origin.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Destination.TemplateName, discreteContext));
                    verb.TryModify(origin);
                }
                else
                {
                    Lexica direction = new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, DirectionType.ToString(), discreteContext);
                    Lexica origin = new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Origin.TemplateName, discreteContext);
                    origin.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, Destination.TemplateName, discreteContext));
                    direction.TryModify(origin);
                }

                me.Event.TryModify(verb);
            }

            return new LexicalParagraph(me);
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

            LexicalContext collectiveContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.None,
                Tense = LexicalTense.Present
            };

            LexicalContext discreteContext = new LexicalContext(viewer)
            {
                Determinant = false,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Near,
                Tense = LexicalTense.Present
            };

            List<ISensoryEvent> sensoryOutput = new List<ISensoryEvent>();
            foreach (MessagingType sense in sensoryTypes)
            {
                SensoryEvent me = new SensoryEvent(new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", collectiveContext), 0, sense);
                ILexica senseVerb = null;
                IEnumerable<ISensoryEvent> senseDescs = Enumerable.Empty<ISensoryEvent>();

                switch (sense)
                {
                    case MessagingType.Audible:
                        me.Strength = GetAudibleDelta(viewer);

                        senseVerb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", collectiveContext);

                        IEnumerable<ISensoryEvent> audibleDescs = GetAudibleDescriptives(viewer);

                        if (audibleDescs.Count() == 0)
                        {
                            continue;
                        }

                        ISensoryEvent audibleNoun = null;
                        if (!audibleDescs.Any(desc => desc.Event.Role == GrammaticalType.DirectObject))
                        {
                            audibleNoun = new SensoryEvent(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "noise", discreteContext), me.Strength, sense);
                        }
                        else
                        {
                            audibleNoun = audibleDescs.FirstOrDefault(desc => desc.Event.Role == GrammaticalType.DirectObject);
                        }

                        audibleNoun.TryModify(audibleDescs.Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                        senseDescs = new List<ISensoryEvent>() { audibleNoun };
                        break;
                    case MessagingType.Olefactory:
                        me.Strength = GetOlefactoryDelta(viewer);

                        senseVerb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", collectiveContext);

                        IEnumerable<ISensoryEvent> smellDescs = GetOlefactoryDescriptives(viewer);

                        if (smellDescs.Count() == 0)
                        {
                            continue;
                        }

                        ISensoryEvent smellNoun = null;
                        if (!smellDescs.Any(desc => desc.Event.Role == GrammaticalType.DirectObject))
                        {
                            smellNoun = new SensoryEvent(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "odor", discreteContext), me.Strength, sense);
                        }
                        else
                        {
                            smellNoun = smellDescs.FirstOrDefault(desc => desc.Event.Role == GrammaticalType.DirectObject);
                        }

                        smellNoun.TryModify(smellDescs.Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                        senseDescs = new List<ISensoryEvent>() { smellNoun };
                        break;
                    case MessagingType.Psychic:
                        me.Strength = GetPsychicDelta(viewer);

                        senseVerb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "sense", collectiveContext);

                        IEnumerable<ISensoryEvent> psyDescs = GetPsychicDescriptives(viewer);

                        if (psyDescs.Count() == 0)
                        {
                            continue;
                        }

                        ISensoryEvent psyNoun = null;
                        if (!psyDescs.Any(desc => desc.Event.Role == GrammaticalType.DirectObject))
                        {
                            psyNoun = new SensoryEvent(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "presence", discreteContext), me.Strength, sense);
                        }
                        else
                        {
                            psyNoun = psyDescs.FirstOrDefault(desc => desc.Event.Role == GrammaticalType.DirectObject);
                        }

                        psyNoun.TryModify(psyDescs.Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                        senseDescs = new List<ISensoryEvent>() { psyNoun };
                        break;
                    case MessagingType.Tactile:
                    case MessagingType.Taste:
                        continue;
                    case MessagingType.Visible:
                        me.Strength = GetVisibleDelta(viewer);

                        senseVerb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", collectiveContext);

                        IEnumerable<ISensoryEvent> seeDescs = GetVisibleDescriptives(viewer);

                        if (seeDescs.Count() == 0)
                        {
                            continue;
                        }

                        ISensoryEvent seeNoun = null;
                        if (!seeDescs.Any(desc => desc.Event.Role == GrammaticalType.DirectObject))
                        {
                            seeNoun = new SensoryEvent(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "thing", discreteContext), me.Strength, sense);
                        }
                        else
                        {
                            seeNoun = seeDescs.FirstOrDefault(desc => desc.Event.Role == GrammaticalType.DirectObject);
                        }

                        seeNoun.TryModify(seeDescs.Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                        senseDescs = new List<ISensoryEvent>() { seeNoun };
                        break;
                }

                if (senseVerb != null && senseDescs.Count() > 0)
                {
                    IEnumerable<ILexica> senseEvents = senseDescs.Select(desc => desc.Event);

                    foreach (ILexica evt in senseEvents)
                    {
                        evt.Context = discreteContext;
                        senseVerb.TryModify(evt);
                    }

                    me.TryModify(senseVerb);
                    sensoryOutput.Add(me);
                }
            }

            return new LexicalParagraph(sensoryOutput);
        }

        /// <summary>
        /// Get the live version of this in the world
        /// </summary>
        /// <returns>The live data</returns>
        public IPathway GetLiveInstance()
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
