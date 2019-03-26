using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Architectural;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Architectural.EntityBase;
using NetMud.Data.Linguistic;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Celestial bodies
    /// </summary>
    [Serializable]
    public class Celestial : LookupDataPartial, ICelestial
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// Orbit Type
        /// </summary>
        [Display(Name = "Orbital Orientation", Description = "What type of orbit this has. Heliocentric means the world orbits this.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public CelestialOrientation OrientationType { get; set; }

        /// <summary>
        /// Zenith distance of an elliptical orbit
        /// </summary>
        [IntDataIntegrity("Apogee must be between 0 and 1,000,000.", 0, 1000000)]
        [Display(Name = "Apogee", Description = "Maximal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        [DataType(DataType.Text)]
        [Required]
        public int Apogee { get; set; }

        /// <summary>
        /// Minimal distance of an elliptical orbit
        /// </summary>
        [IntDataIntegrity("Perigree must be between 0 and 1,000,000.", 0, 1000000)]
        [Display(Name = "Perigree", Description = "Minimal distance this is from the world it orbits. (eliptical orbits only, this is averaged with Perigree for circular orbits)")]
        [DataType(DataType.Text)]
        [Required]
        public int Perigree { get; set; }

        /// <summary>
        /// How fast is this going through space
        /// </summary>
        [IntDataIntegrity("Velocity must be between 0 and 10000.", 0, 10000)]
        [Display(Name = "Velocity", Description = "How fast is this hurtling through space. (affects a LOT of things)")]
        [DataType(DataType.Text)]
        [Required]
        public int Velocity { get; set; }

        /// <summary>
        /// How bright is this thing
        /// </summary>
        [IntDataIntegrity("Luminosity must be between 0 and 10000.", 0, 10000)]
        [Display(Name = "Luminosity", Description = "How bright is this. Measured in thousands. Anything less than 1000 is not visible to the naked eye.")]
        [DataType(DataType.Text)]
        public int Luminosity { get; set; }

        /// <summary>
        /// Framework for the physics model of an entity
        /// </summary>
        [NonNullableDataIntegrity("Physical model is invalid.")]
        [UIHint("TwoDimensionalModel")]
        public IDimensionalModel Model { get; set; }

        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<ISensoryEvent> Descriptives { get; set; }

        public Celestial()
        {
            Model = new DimensionalModel();
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public ILexicalParagraph GetFullDescription(IEntity viewer, MessagingType[] sensoryTypes = null)
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
                    case MessagingType.Olefactory:
                    case MessagingType.Psychic:
                    case MessagingType.Tactile:
                    case MessagingType.Taste:
                    case MessagingType.Visible:
                        self.Strength = GetVisibleDelta(viewer);

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
        public ISensoryEvent GetImmediateDescription(IEntity viewer, MessagingType sensoryType)
        {
            ISensoryEvent me = GetSelf(sensoryType);
            switch (sensoryType)
            {
                case MessagingType.Audible:
                case MessagingType.Olefactory:
                case MessagingType.Psychic:
                case MessagingType.Tactile:
                case MessagingType.Taste:
                    break;
                case MessagingType.Visible:
                    me.Strength = GetVisibleDelta(viewer);
                    me.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            if (me.Event.Modifiers.Any())
            {
                return me;
            }

            return new SensoryEvent(sensoryType);
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public string GetDescribableName(IEntity viewer)
        {
            var strength = GetVisibleDelta(viewer);

            return GetSelf(MessagingType.Visible, strength).ToString();
        }

        internal ISensoryEvent GetSelf(MessagingType type, short strength = 30)
        {
            return new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = Name, Type = LexicalType.ProperNoun, Role = GrammaticalType.Subject }
            };
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }

        /// <summary>
        /// Render this as being show inside a container
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public ILexicalParagraph RenderAsContents(IEntity viewer, MessagingType[] sensoryTypes)
        {
            if (sensoryTypes == null || sensoryTypes.Count() == 0)
            {
                sensoryTypes = new MessagingType[] { MessagingType.Audible, MessagingType.Olefactory, MessagingType.Psychic, MessagingType.Tactile, MessagingType.Taste, MessagingType.Visible };
            }

            IList<ISensoryEvent> Messages = new List<ISensoryEvent>();
            //Self becomes the first sense in the list
            foreach (MessagingType sense in sensoryTypes)
            {
                ISensoryEvent me = GetSelf(sense);
                switch (sense)
                {
                    case MessagingType.Audible:
                    case MessagingType.Olefactory:
                    case MessagingType.Psychic:
                    case MessagingType.Tactile:
                    case MessagingType.Taste:
                        continue;
                    case MessagingType.Visible:
                        me.Strength = GetVisibleDelta(viewer);

                        me.TryModify(GetVisibleDescriptives(viewer));

                        me.Event.Context = new LexicalContext(viewer)
                        {
                            Determinant = true,
                            Perspective = NarrativePerspective.None,
                            Plural = false,
                            Position = LexicalPosition.InsideOf,
                            Tense = LexicalTense.Present
                        };

                        var skyContext = new LexicalContext(viewer)
                        {
                            Determinant = true,
                            Perspective = NarrativePerspective.None,
                            Plural = false,
                            Position = LexicalPosition.None,
                            Tense = LexicalTense.Present
                        };

                        me.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "sky", skyContext));
                        break;
                }

                if (me.Event.Modifiers.Count() > 0)
                {
                    Messages.Add(me);
                }
            }

            return new LexicalParagraph(Messages);
        }

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision Range taking into account blindness and other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public ValueRange<float> GetVisualRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>If this is visible</returns>
        public short GetVisibleDelta(IEntity viewer, short modifier = 0)
        {
            if (viewer != null)
            {
                float value = Luminosity; //TODO: make this based on outside conditions
                ValueRange<float> range = viewer.GetVisualRange();

                var lowDelta = value - (range.Low - modifier);
                var highDelta = (range.High + modifier) - value;

                if (lowDelta < 0)
                {
                    return (short)Math.Max(-100, lowDelta);
                }

                if (highDelta < 0)
                {
                    return (short)Math.Min(100, Math.Abs(highDelta));
                }
            }

            return 0;
        }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this)
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public ILexicalParagraph RenderToVisible(IEntity viewer)
        {
            return GetFullDescription(viewer, new MessagingType[] { MessagingType.Visible });
        }
        #endregion

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("Celestial Orientation", OrientationType.ToString());
            returnList.Add("Apogee", Apogee.ToString());
            returnList.Add("Perigree", Perigree.ToString());
            returnList.Add("Velocity", Velocity.ToString());
            returnList.Add("Luminosity", Luminosity.ToString());

            return returnList;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Celestial
            {
                Name = Name,
                HelpText = HelpText,
                Apogee = Apogee,
                Perigree = Perigree,
                Velocity = Velocity,
                Luminosity = Luminosity,
                OrientationType = OrientationType
            };
        }
    }
}
