using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Gaia.Geographical;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Inanimate
{
    /// <summary>
    /// "Object" class
    /// </summary>
    [Serializable]
    public class Inanimate : LocationEntityPartial, IInanimate
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
                return Template<IInanimateTemplate>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(IInanimateTemplate), TemplateId));
        }

        /// <summary>
        /// How many of this can be in a stack
        /// </summary>
        [Display(Name = "Accumulation Cap", Description = "How many of this can go in one 'stack'.")]
        [Range(0, 999, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Required]
        public int AccumulationCap { get; set; }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        public IDimensionalModel Model { get; set; }
        #endregion

        [JsonConstructor]
        public Inanimate()
        {
            //IDatas need parameterless constructors
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();
        }

        /// <summary>
        /// News up an entity with its backing data
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        public Inanimate(IInanimateTemplate backingStore)
        {
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();

            TemplateId = backingStore.Id;
        }

        /// <summary>
        /// News up an entity with its backing data and where to spawn it into
        /// </summary>
        /// <param name="backingStore">the backing data</param>
        /// <param name="spawnTo">where to spawn this into</param>
        public Inanimate(IInanimateTemplate backingStore, IGlobalPosition spawnTo)
        {
            Contents = new EntityContainer<IInanimate>();
            Qualities = new HashSet<IQuality>();

            TemplateId = backingStore.Id;
            SpawnNewInWorld(spawnTo);
        }

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        public override float GetCurrentLuminosity()
        {
            float lumins = 0;
            foreach (IInanimate thing in Contents.EntitiesContained())
            {
                lumins += thing.GetCurrentLuminosity();
            }

            return lumins;
        }

        /// <summary>
        /// Get's the entity's model dimensions
        /// </summary>
        /// <returns>height, length, width</returns>
        public override Dimensions GetModelDimensions()
        {
            return new Dimensions(Model.Height, Model.Length, Model.Width);
        }

        public override IGlobalPosition GetContainerAsLocation()
        {
            return new GlobalPosition(CurrentLocation.CurrentZone, CurrentLocation.CurrentLocale, CurrentLocation.CurrentRoom) { CurrentContainer = this };
        }

        #region spawning
        /// <summary>
        /// Spawn this new into the live world
        /// </summary>
        public override void SpawnNewInWorld()
        {
            if (CurrentLocation == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            SpawnNewInWorld(CurrentLocation);
        }

        /// <summary>
        /// Spawn this new into the live world into a specified container
        /// </summary>
        /// <param name="spawnTo">the location/container this should spawn into</param>
        public override void SpawnNewInWorld(IGlobalPosition spawnTo)
        {
            //We can't even try this until we know if the data is there
            IInanimateTemplate bS = Template<IInanimateTemplate>() ?? throw new InvalidOperationException("Missing backing data store on object spawn event.");

            Keywords = bS.Keywords;

            if (string.IsNullOrWhiteSpace(BirthMark))
            {
                BirthMark = LiveCache.GetUniqueIdentifier(bS);
                Birthdate = DateTime.Now;
            }

            Qualities = bS.Qualities;
            AccumulationCap = bS.AccumulationCap;

            TryMoveTo(spawnTo);

            if (CurrentLocation == null)
            {
                throw new NotImplementedException("Objects can't spawn to nothing");
            }

            UpsertToLiveWorldCache(true);

            KickoffProcesses();
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

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Inanimate
            {
                Qualities = Qualities,
                TemplateId = TemplateId
            };
        }
        #endregion

        #region rendering
        public override IMessage RenderAsWorn(IEntity viewer, IEntity wearer)
        {
            //TODO: Worn position
            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        public override IMessage RenderAsHeld(IEntity viewer, IEntity holder)
        {
            //TODO: Worn position
            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        /// <summary>
        /// Renders HTML for the info card popups
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the output HTML</returns>
        public override string RenderToInfo(IEntity viewer)
        {
            if (viewer == null)
            {
                return string.Empty;
            }

            IInanimateTemplate dt = Template<IInanimateTemplate>();
            StringBuilder sb = new StringBuilder();
            StaffRank rank = viewer.ImplementsType<IPlayer>() ? viewer.Template<IPlayerTemplate>().GamePermissionsRank : StaffRank.Player;

            sb.Append("<div class='helpItem'>");

            sb.AppendFormat("<h3>{0}</h3>", GetDescribableName(viewer));
            sb.Append("<hr />");

            if (Qualities.Count > 0)
            {
                sb.Append("<h4>Qualities</h4>");
                sb.AppendFormat("<div>{0}</div>", string.Join(",", Qualities.Select(q => string.Format("({0}:{1})", q.Name, q.Value))));
            }

            sb.Append("</div>");

            return sb.ToString();
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public override IEnumerable<IMessage> GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            var collectiveContext = new LexicalContext()
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Around,
                Tense = LexicalTense.Present
            };

            var discreteContext = new LexicalContext()
            {
                Determinant = true,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            //Self becomes the first sense in the list
            List<IMessage> messages = new List<IMessage>();
            foreach (MessagingType sense in sensoryTypes)
            {
                var me = GetSelf(sense);

                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> aDescs = GetAudibleDescriptives(viewer);

                        me.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        ILexica uberSounds = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", collectiveContext);
                        uberSounds.TryModify(aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in aDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.Subject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            me.TryModify(newDesc);
                        }

                        if (uberSounds.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSounds);
                        }

                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> oDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica uberSmells = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", collectiveContext);
                        uberSmells.TryModify(oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in oDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSmells.TryModify(newDesc);
                        }

                        if (uberSmells.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSmells);
                        }

                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> pDescs = GetPsychicDescriptives(viewer);

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

                        if (uberPsy.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(collectivePsy);
                        }

                        break;
                    case MessagingType.Taste:
                        if (!IsTastableTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> taDescs = GetTasteDescriptives(viewer);

                        me.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica uberTaste = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "taste", collectiveContext);
                        uberTaste.TryModify(taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in taDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTaste.TryModify(newDesc);
                        }

                        if (uberTaste.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberTaste);
                        }

                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> tDescs = GetSmellableDescriptives(viewer);

                        me.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica uberTouch = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "feel", collectiveContext);
                        uberTouch.TryModify(tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in tDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberTouch.TryModify(newDesc);
                        }

                        if (uberTouch.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberTouch);
                        }

                        break;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                        {
                            continue;
                        }

                        IEnumerable<ISensoryEvent> vDescs = GetVisibleDescriptives(viewer);

                        me.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Descriptive));

                        Lexica uberSight = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "appears", collectiveContext);
                        uberSight.TryModify(vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.DirectObject).Select(adesc => adesc.Event));

                        foreach (ISensoryEvent desc in vDescs.Where(adesc => adesc.Event.Role == GrammaticalType.Subject))
                        {
                            Lexica newDesc = new Lexica(desc.Event.Type, GrammaticalType.DirectObject, desc.Event.Phrase, discreteContext);
                            newDesc.TryModify(desc.Event.Modifiers);

                            uberSight.TryModify(newDesc);
                        }

                        if (uberSight.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                        {
                            me.TryModify(uberSight);
                        }

                        //Describe the size and population of this zone
                        DimensionalSizeDescription objectSize = GeographicalUtilities.ConvertSizeToType(GetModelDimensions(), GetType());

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, objectSize.ToString());

                        //Render people in the zone
                        ObjectContainmentSizeDescription bulgeSizeAdjective = GeographicalUtilities.GetObjectContainmentSize(GetContents<IInanimate>().Sum(obj => obj.GetModelVolume()), GetModelVolume());

                        me.TryModify(LexicalType.Adjective, GrammaticalType.Descriptive, bulgeSizeAdjective.ToString());

                        break;
                }

                messages.Add(new Message(sense, me));
            }

            return messages;
        }

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        public override IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer)
        {
            //No celestials inside an object
            return Enumerable.Empty<ICelestial>();
        }
        #endregion
    }
}
