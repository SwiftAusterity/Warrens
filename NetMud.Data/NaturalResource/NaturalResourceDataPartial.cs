using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Linguistic;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.NaturalResource
{
    /// <summary>
    /// Partial class for handling the basics of natural resources (rocks, trees, etc)
    /// </summary>
    [Serializable]
    public abstract class NaturalResourceDataPartial : LookupDataPartial, INaturalResource
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// How much spawns in one place in one spawn tick
        /// </summary>
        [IntDataIntegrity("Amount Multiplier must be between 0 and 100.", 0, 100)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Multiplier", Description = "The factor that governs how much of this spawns in a new location.")]
        [DataType(DataType.Text)]
        public int AmountMultiplier { get; set; }

        /// <summary>
        /// How rare this is to spawn even in its optimal range
        /// </summary>
        [IntDataIntegrity("Rarity must be between 0 and 100.", 0, 100)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Spawn Rarity", Description = "How rare is this to spawn at all in a new location.")]
        [DataType(DataType.Text)]
        public int Rarity { get; set; }

        /// <summary>
        /// How much the spawned puissance varies
        /// </summary>
        [IntDataIntegrity("Puissance Variance must be between 0 and 100.", 0, 100)]
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Puissance Variance", Description = "How much deviation in random magical strength will be spawned in.")]
        [DataType(DataType.Text)]
        public int PuissanceVariance { get; set; }

        /// <summary>
        /// Spawns in elevations within this range
        /// </summary>
        [Range(-1000, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Elevation", Description = "The upper elevation cap this will allow to spawn in.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> ElevationRange { get; set; }

        /// <summary>
        /// Spawns in temperatures within this range
        /// </summary>
        [Range(-2000, 2000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Temperature", Description = "The upper temperature cap this will allow to spawn in.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> TemperatureRange { get; set; }

        /// <summary>
        /// Spawns in humidities within this range
        /// </summary>
        [Range(0, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Humidity", Description = "The upper barometric pressure cap this will allow to spawn in.")]
        [UIHint("ValueRangeInt")]
        public ValueRange<int> HumidityRange { get; set; }

        /// <summary>
        /// What medium biomes this can spawn in
        /// </summary>
        [FilledContainerDataIntegrity("This resource must occur in at least one biome.")]
        [Display(Name = "Occurs in Biome", Description = "What biomes this will allow to spawn in.")]
        [UIHint("OccursIn")]
        public HashSet<Biome> OccursIn { get; set; }

        /// <summary>
        /// Can spawn in system zones like non-player owned cities
        /// </summary>
        [Display(Name = "System Area Spawning", Description = "Can spawn in system zones like non-player owned cities.")]
        public bool CanSpawnInSystemAreas { get; set; }

        /// <summary>
        /// Can this resource potentially spawn in this room
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this can spawn there</returns>
        public virtual bool CanSpawnIn(IGlobalPosition position)
        {
            return true;
        }

        /// <summary>
        /// Should this resource spawn in this room. Combines the "can" logic with checks against total local population
        /// </summary>
        /// <param name="room">The room to spawn in</param>
        /// <returns>if this should spawn there</returns>
        public virtual bool ShouldSpawnIn(IGlobalPosition room)
        {
            return true;
        }

        /// <summary>
        /// Gets the errors for data fitness
        /// </summary>
        /// <returns>a bunch of text saying how awful your data is</returns>
        public override IList<string> FitnessReport()
        {
            IList<string> dataProblems = base.FitnessReport();

            //Tuples must be handled individually
            if (ElevationRange.Low < 0 || ElevationRange.High < ElevationRange.Low)
            {
                dataProblems.Add("Elevation Range is incorrect.");
            }

            if (TemperatureRange.Low < 0 || TemperatureRange.High < TemperatureRange.Low)
            {
                dataProblems.Add("Temperature Range is incorrect.");
            }

            if (HumidityRange.Low < 0 || HumidityRange.High < HumidityRange.Low)
            {
                dataProblems.Add("Humidity Range is incorrect.");
            }

            return dataProblems;
        }

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Amount Multiplier", AmountMultiplier.ToString());
            returnList.Add("Rarity", Rarity.ToString());
            returnList.Add("Puissance Variance", PuissanceVariance.ToString());
            returnList.Add("Elevation", string.Format("{0} - {1}", ElevationRange.Low, ElevationRange.High));
            returnList.Add("Temperature", string.Format("{0} - {1}", TemperatureRange.Low, TemperatureRange.High));
            returnList.Add("Humidity", string.Format("{0} - {1}", HumidityRange.Low, HumidityRange.High));

            foreach (Biome occur in OccursIn)
            {
                returnList.Add("Occurs In", occur.ToString());
            }

            return returnList;
        }

        #region Generic Rendering
        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<ISensoryEvent> Descriptives { get; set; }

        /// <summary>
        /// Renders output for this entity when Look targets it
        /// </summary>
        /// <param name="actor">entity initiating the command</param>
        /// <returns>the output</returns>
        public virtual IMessage RenderToTrack(IEntity actor)
        {
            //Default for "tracking" is null
            return null;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<IMessage> GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Self becomes the first sense in the list
            IList<IMessage> messages = new List<IMessage>();
            foreach (MessagingType sense in sensoryTypes)
            {
                var self = GetSelf(sense);
                switch (sense)
                {
                    case MessagingType.Audible:
                        if (!IsAudibleTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetAudibleDescriptives(viewer));
                        break;
                    case MessagingType.Olefactory:
                        if (!IsSmellableTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetSmellableDescriptives(viewer));
                        break;
                    case MessagingType.Psychic:
                        if (!IsSensibleTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetPsychicDescriptives(viewer));
                        break;
                    case MessagingType.Tactile:
                        if (!IsTouchableTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetTouchDescriptives(viewer));
                        break;
                    case MessagingType.Taste:
                        if (!IsTastableTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetTasteDescriptives(viewer));
                        break;
                    case MessagingType.Visible:
                        if (!IsVisibleTo(viewer))
                        {
                            continue;
                        }

                        self.Occurrence.TryModify(GetVisibleDescriptives(viewer));
                        break;
                }

                messages.Add(self);
            }

            return messages;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage GetImmediateDescription(IEntity viewer, MessagingType sense)
        {
            var me = GetSelf(sense);
            switch (sense)
            {
                case MessagingType.Audible:
                    if (!IsAudibleTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetAudibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Olefactory:
                    if (!IsSmellableTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetSmellableDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Psychic:
                    if (!IsSensibleTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetPsychicDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Tactile:
                    if (!IsTouchableTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetTouchDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Taste:
                    if (!IsTastableTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetTasteDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Visible:
                    if (!IsVisibleTo(viewer))
                    {
                        return new Message(sense, new SensoryEvent(sense));
                    }

                    me.Occurrence.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            return me;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
            {
                return string.Empty;
            }

            return GetSelf(MessagingType.Visible).ToString();
        }

        internal IMessage GetSelf(MessagingType type, int strength = 100)
        {
            return new Message(type, new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = Name, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            });
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public virtual bool IsVisibleTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetVisualRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<IMessage> RenderToLook(IEntity viewer)
        {
            if (!IsVisibleTo(viewer))
            {
                return null;
            }

            return GetFullDescription(viewer, new[] { MessagingType.Visible, MessagingType.Psychic, MessagingType.Olefactory });
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual IMessage RenderToScan(IEntity viewer)
        {
            //TODO: Make this half power
            if (!IsVisibleTo(viewer))
            {
                return null;
            }

            return GetImmediateDescription(viewer, MessagingType.Visible);
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual IEnumerable<IMessage> RenderToInspect(IEntity viewer)
        {
            //TODO: Make this double power
            if (!IsVisibleTo(viewer))
            {
                return null;
            }

            return GetFullDescription(viewer);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }
        #endregion

        #region Auditory Rendering
        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsAudibleTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetAuditoryRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage RenderToAudible(IEntity viewer)
        {
            if (!IsAudibleTo(viewer))
            {
                return null;
            }

            var self = GetSelf(MessagingType.Audible);

            foreach (ISensoryEvent descriptive in GetAudibleDescriptives(viewer))
            {
                self.Occurrence.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetAudibleDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Audible);
        }
        #endregion

        #region Psychic (sense) Rendering
        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSensibleTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetPsychicRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage RenderToSense(IEntity viewer)
        {
            if (!IsSensibleTo(viewer))
            {
                return null;
            }

            var self = GetSelf(MessagingType.Psychic);

            foreach (ISensoryEvent descriptive in GetPsychicDescriptives(viewer))
            {
                self.Occurrence.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetPsychicDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Psychic);
        }
        #endregion

        #region Taste Rendering
        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTastableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetTasteRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage RenderToTaste(IEntity viewer)
        {
            if (!IsTastableTo(viewer))
            {
                return null;
            }

            var self = GetSelf(MessagingType.Taste);

            foreach (ISensoryEvent descriptive in GetTasteDescriptives(viewer))
            {
                self.Occurrence.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTasteDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Taste);
        }
        #endregion

        #region Smell Rendering
        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsSmellableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetOlefactoryRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage RenderToSmell(IEntity viewer)
        {
            if (!IsSmellableTo(viewer))
            {
                return null;
            }

            var self = GetSelf(MessagingType.Olefactory);

            foreach (ISensoryEvent descriptive in GetSmellableDescriptives(viewer))
            {
                self.Occurrence.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetSmellableDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Olefactory);
        }
        #endregion

        #region Touch Rendering
        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>If this is observable</returns>
        public virtual bool IsTouchableTo(IEntity viewer)
        {
            int value = 0;
            ValueRange<float> range = viewer.GetTactileRange();

            return value >= range.Low && value <= range.High;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IMessage RenderToTouch(IEntity viewer)
        {
            if (!IsTouchableTo(viewer))
            {
                return null;
            }

            var self = GetSelf(MessagingType.Tactile);

            foreach (ISensoryEvent descriptive in GetTouchDescriptives(viewer))
            {
                self.Occurrence.TryModify(descriptive.Event);
            }

            return self;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTouchDescriptives(IEntity viewer)
        {
            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Tactile);
        }
        #endregion

        #region Containment Rendering
        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual IEnumerable<IMessage> RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Add the existential modifiers
            var me = GetImmediateDescription(viewer, sensoryTypes[0]);
            me.Occurrence.TryModify(LexicalType.Noun, GrammaticalType.DirectObject, "ground")
                .TryModify(
                    new Tuple<LexicalType, GrammaticalType, string>[] {
                                new Tuple<LexicalType, GrammaticalType, string>(LexicalType.Article, GrammaticalType.IndirectObject, "in")
                        }
                    );

            return new IMessage[] { me };
        }

        public virtual IEnumerable<IMessage> RenderResourceCollection(IEntity viewer, int amount)
        {
            var collectiveContext = new LexicalContext()
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Around,
                Tense = LexicalTense.Present
            };

            var me = GetImmediateDescription(viewer, MessagingType.Visible);
            me.Occurrence.TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, amount.ToString(), collectiveContext));

            return new IMessage[] { me };
        }
        #endregion
    }
}
