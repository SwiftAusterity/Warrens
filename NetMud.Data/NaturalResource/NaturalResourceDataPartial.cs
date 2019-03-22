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
        public virtual ILexicalParagraph RenderToTrack(IEntity actor)
        {
            //Default for "tracking" is null
            return null;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            IList<ISensoryEvent> Messages = new List<ISensoryEvent>();
            //Self becomes the first sense in the list
            foreach (MessagingType sense in sensoryTypes)
            {
                ISensoryEvent self = GetSelf(sense);

                switch (sense)
                {
                    case MessagingType.Audible:
                        self.Strength = (GetAudibleDelta(viewer) * 30);

                        self.TryModify(GetAudibleDescriptives(viewer));
                        break;
                    case MessagingType.Olefactory:
                        self.Strength = (GetSmellDelta(viewer) * 30);

                        self.TryModify(GetSmellableDescriptives(viewer));
                        break;
                    case MessagingType.Psychic:
                        self.Strength = (GetPsychicDelta(viewer) * 30);

                        self.TryModify(GetPsychicDescriptives(viewer));
                        break;
                    case MessagingType.Tactile:
                        self.Strength = (GetTactileDelta(viewer) * 30);

                        self.TryModify(GetTouchDescriptives(viewer));
                        break;
                    case MessagingType.Taste:
                        self.Strength = (GetTasteDelta(viewer) * 30);

                        self.TryModify(GetTasteDescriptives(viewer));
                        break;
                    case MessagingType.Visible:
                        self.Strength = (GetVisibleDelta(viewer) * 30);

                        self.TryModify(GetVisibleDescriptives(viewer));
                        break;
                }

                if (self.Event.Modifiers.Count() > 0)
                {
                    Messages.Add(self);
                }
            }

            return new LexicalParagraph(Messages);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ISensoryEvent GetImmediateDescription(IEntity viewer, MessagingType sense)
        {
            var self = GetSelf(sense);
            switch (sense)
            {
                case MessagingType.Audible:
                    self.Strength = (GetAudibleDelta(viewer) * 30);

                    self.TryModify(GetAudibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Olefactory:
                    self.Strength = (GetSmellDelta(viewer) * 30);

                    self.TryModify(GetSmellableDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Psychic:
                    self.Strength = (GetPsychicDelta(viewer) * 30);

                    self.TryModify(GetPsychicDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Tactile:
                    self.Strength = (GetTactileDelta(viewer) * 30);

                    self.TryModify(GetTouchDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Taste:
                    self.Strength = (GetTasteDelta(viewer) * 30);

                    self.TryModify(GetTasteDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Visible:
                    self.Strength = (GetVisibleDelta(viewer) * 30);

                    self.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            return self;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual string GetDescribableName(IEntity viewer)
        {
            if (GetVisibleDelta(viewer) != 0)
            {
                return string.Empty;
            }

            return GetSelf(MessagingType.Visible).ToString();
        }

        internal ISensoryEvent GetSelf(MessagingType type, int strength = 30)
        {
            return new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = Name, Type = LexicalType.Noun, Role = GrammaticalType.Subject }
            };
        }
        #endregion

        #region Visual Rendering
        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public virtual short GetVisibleDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetVisualRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToLook(IEntity viewer)
        {
            return GetFullDescription(viewer, new[] { MessagingType.Visible, MessagingType.Psychic, MessagingType.Olefactory });
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ILexicalParagraph RenderToScan(IEntity viewer)
        {
            return new LexicalParagraph(GetImmediateDescription(viewer, MessagingType.Visible));
        }

        /// <summary>
        /// Renders "display" from scan command
        /// </summary>
        /// <param name="viewer">entity initiating the command</param>
        /// <returns>the scan output</returns>
        public virtual ILexicalParagraph RenderToInspect(IEntity viewer)
        {
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
        public virtual short GetAudibleDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetAuditoryRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToAudible(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Audible);
            self.Strength = (GetAudibleDelta(viewer) * 30);
            self.TryModify(GetAudibleDescriptives(viewer));

            return new LexicalParagraph(self);
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
        public virtual short GetPsychicDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetPsychicRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToSense(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Psychic);
            self.Strength = (GetPsychicDelta(viewer) * 30);
            self.TryModify(GetPsychicDescriptives(viewer));

            return new LexicalParagraph(self);
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
        public virtual short GetTasteDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetTasteRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToTaste(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Taste);
            self.Strength = (GetTasteDelta(viewer) * 30);

            self.TryModify(GetTasteDescriptives(viewer));

            return new LexicalParagraph(self);
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
        public virtual short GetSmellDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetOlefactoryRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToSmell(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Olefactory);
            self.Strength = (GetSmellDelta(viewer) * 30);
            self.TryModify(GetSmellableDescriptives(viewer));

            return new LexicalParagraph(self);
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
        public virtual short GetTactileDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                int value = 0;
                ValueRange<float> range = viewer.GetTactileRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public virtual ILexicalParagraph RenderToTouch(IEntity viewer)
        {
            ISensoryEvent self = GetSelf(MessagingType.Tactile);
            self.Strength = (GetTactileDelta(viewer) * 30);
            self.TryModify(GetTouchDescriptives(viewer));

            return new LexicalParagraph(self);
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
        public virtual ILexicalParagraph RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            //Add the existential modifiers
            var me = GetImmediateDescription(viewer, sensoryTypes[0]);
            me.TryModify(LexicalType.Noun, GrammaticalType.DirectObject, "ground")
                .TryModify(
                    new Tuple<LexicalType, GrammaticalType, string>[] {
                                new Tuple<LexicalType, GrammaticalType, string>(LexicalType.Article, GrammaticalType.IndirectObject, "in")
                        }
                    );

            return new LexicalParagraph(me);
        }

        public virtual ILexicalParagraph RenderResourceCollection(IEntity viewer, int amount)
        {
            var collectiveContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Around,
                Tense = LexicalTense.Present
            };

            var me = GetImmediateDescription(viewer, MessagingType.Visible);
            me.TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, amount.ToString(), collectiveContext));

            return new LexicalParagraph(me);
        }
        #endregion
    }
}
