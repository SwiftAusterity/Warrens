using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.Data.Linguistic;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Zone
{
    /// <summary>
    /// Individual weather events
    /// </summary>
    [Serializable]
    public class WeatherEvent : IWeatherEvent
    {
        /// <summary>
        /// The event type
        /// </summary>
        [Display(Name = "Type", Description = "The type of weather.")]
        [UIHint("EnumDropDownList")]
        public WeatherEventType Type { get; set; }

        /// <summary>
        /// The strength of the event (size of cloud, windspeed for cyclones)
        /// </summary>
        [Display(Name = "Strength", Description = "The type of weather.")]
        [DataType(DataType.Text)]
        public float Strength { get; set; }

        /// <summary>
        /// How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks
        /// </summary>
        [Display(Name = "Drain", Description = "How much strength does this bleed per cycle, important for cyclones and typhoons and for earthquake aftershocks.")]
        [DataType(DataType.Text)]
        public float Drain { get; set; }

        /// <summary>
        /// How high up is the event
        /// </summary>
        [Display(Name = "Altitude", Description = "How high up is the event.")]
        [DataType(DataType.Text)]
        public float Altitude { get; set; }

        /// <summary>
        /// How much does this obscure the sky, Percentage
        /// </summary>
        [Display(Name = "Coverage", Description = "THow much does this obscure the sky, Percentage.")]
        [DataType(DataType.Text)]
        public float Coverage { get; set; }

        /// <summary>
        /// How much precipitation does this produce per cycle?
        /// </summary>
        [Display(Name = "PrecipitationAmount", Description = "How much precipitation does this produce per cycle?.")]
        [DataType(DataType.Text)]
        public float PrecipitationAmount { get; set; }

        #region Visual Rendering
        /// <summary>
        /// Gets the actual vision Range taking into account blindness and other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetVisualRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this visible to the viewer
        /// </summary>
        /// <param name="viewer">the viewing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
        public virtual short GetVisibleDelta(IEntity viewer)
        {
            if (viewer != null)
            {
                //TODO: make this based on outside conditions
                float value = 30;
                ValueRange<float> range = viewer.GetVisualRange();

                return value < range.Low ? (short)(value - range.Low)
                    : value > range.High ? (short)(value - range.High)
                    : (short)0;
            }

            return 0;
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetVisibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Visible);
        }
        #endregion

        #region Auditory Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetAuditoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
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
            self.Strength = 30 + (GetAudibleDelta(viewer) * 30);
            self.TryModify(GetAudibleDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged as visible output
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetAudibleDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Audible);
        }
        #endregion
        #region Smell Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetOlefactoryRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
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
            self.Strength = 30 + (GetSmellDelta(viewer) * 30);
            self.TryModify(GetSmellableDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetSmellableDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Olefactory);
        }
        #endregion

        #region Touch Rendering
        /// <summary>
        /// Gets the actual Range taking into account other factors
        /// </summary>
        /// <returns>the working Range</returns>
        public virtual ValueRange<float> GetTactileRange()
        {
            //Base is "infinite" for things like rocks and zones
            return new ValueRange<float>(-999999, 999999);
        }

        /// <summary>
        /// Is this detectable to the viewer
        /// </summary>
        /// <param name="viewer">the observing entity</param>
        /// <returns>0 = observable, negative = too low to detect, positive = too high to detect</returns>
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
            self.Strength = 30 + (GetTactileDelta(viewer) * 30);
            self.TryModify(GetTouchDescriptives(viewer));

            return new LexicalParagraph(self);
        }

        /// <summary>
        /// Retrieve all of the descriptors that are tagged
        /// </summary>
        /// <returns>A collection of the descriptors</returns>
        public virtual IEnumerable<ISensoryEvent> GetTouchDescriptives(IEntity viewer)
        {
            if (Descriptives == null)
            {
                return Enumerable.Empty<ISensoryEvent>();
            }

            return Descriptives.Where(desc => desc.SensoryType == MessagingType.Tactile);
        }
        #endregion

        #region Rendering
        /// <summary>
        /// Set of output relevant to this exit. These are essentially single word descriptions to render the path
        /// </summary>
        public HashSet<ISensoryEvent> Descriptives { get; set; }

        /// <summary>
        /// Render this to a look command (what something sees when it 'look's at this
        /// </summary>
        /// <returns>the output strings</returns>
        public ILexicalParagraph RenderToLook(IEntity viewer)
        {
            var strength = 30 + (GetVisibleDelta(viewer) * 30);

            ISensoryEvent me = GetSelf(MessagingType.Visible, strength);

            var collectiveContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.SecondPerson,
                Plural = false,
                Position = LexicalPosition.Near,
                Tense = LexicalTense.Present
            };

            var discreteContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.ThirdPerson,
                Plural = false,
                Position = LexicalPosition.Attached,
                Tense = LexicalTense.Present
            };

            Lexica verb = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "leads", collectiveContext);

            me.Event.TryModify(verb);

            return new LexicalParagraph(me);
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

            var skyContext = new LexicalContext(viewer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.None,
                Plural = false,
                Position = LexicalPosition.None,
                Tense = LexicalTense.Present
            };

            IList<ISensoryEvent> Messages = new List<ISensoryEvent>();
            foreach (var sense in sensoryTypes)
            {
                ISensoryEvent me = GetSelf(sense);

                me.Event.Context = new LexicalContext(viewer)
                {
                    Determinant = true,
                    Perspective = NarrativePerspective.None,
                    Plural = false,
                    Position = LexicalPosition.InsideOf,
                    Tense = LexicalTense.Present
                };

                switch (sense)
                {
                    case MessagingType.Audible:
                        var audibleDescs = GetAudibleDescriptives(viewer);

                        if(audibleDescs.Count() == 0)
                        {
                            continue;
                        }

                        me.Strength = 30 + (GetAudibleDelta(viewer) * 30);

                        me.TryModify(audibleDescs);

                        me.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "sky", skyContext));
                        break;
                    case MessagingType.Olefactory:
                        var smellDescs = GetSmellableDescriptives(viewer);

                        if (smellDescs.Count() == 0)
                        {
                            continue;
                        }

                        me.Strength = 30 + (GetSmellDelta(viewer) * 30);

                        me.TryModify(smellDescs);

                        me.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "sky", skyContext));
                        break;
                    case MessagingType.Tactile:
                        var touchDescs = GetTouchDescriptives(viewer);

                        if (touchDescs.Count() == 0)
                        {
                            continue;
                        }
                        me.Strength = 30 + (GetTactileDelta(viewer) * 30);

                        me.TryModify(touchDescs);

                        me.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.DirectObject, "sky", skyContext));
                        break;
                    case MessagingType.Psychic:
                    case MessagingType.Taste:
                        continue;
                    case MessagingType.Visible:
                        me.Strength = 30 + (GetVisibleDelta(viewer) * 30);

                        me.TryModify(GetVisibleDescriptives(viewer));

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
                        self.Strength = 30 + (GetAudibleDelta(viewer) * 30);

                        self.TryModify(GetAudibleDescriptives(viewer));
                        break;
                    case MessagingType.Olefactory:
                        self.Strength = 30 + (GetSmellDelta(viewer) * 30);

                        self.TryModify(GetSmellableDescriptives(viewer));
                        break;
                    case MessagingType.Psychic:
                    case MessagingType.Taste:
                        break;
                    case MessagingType.Tactile:
                        self.Strength = 30 + (GetTactileDelta(viewer) * 30);

                        self.TryModify(GetTouchDescriptives(viewer));
                        break;
                    case MessagingType.Visible:
                        self.Strength = 30 + (GetVisibleDelta(viewer) * 30);

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
            ISensoryEvent me = GetSelf(sense);
            switch (sense)
            {
                case MessagingType.Audible:
                    me.Strength = 30 + (GetAudibleDelta(viewer) * 30);

                    me.TryModify(GetAudibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Olefactory:
                    me.Strength = 30 + (GetSmellDelta(viewer) * 30);

                    me.TryModify(GetSmellableDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Taste:
                case MessagingType.Psychic:
                    break;
                case MessagingType.Tactile:
                    me.Strength = 30 + (GetTactileDelta(viewer) * 30);

                    me.TryModify(GetTouchDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
                case MessagingType.Visible:
                    me.Strength = 30 + (GetVisibleDelta(viewer) * 30);

                    me.TryModify(GetVisibleDescriptives(viewer).Where(desc => desc.Event.Role == GrammaticalType.Descriptive));
                    break;
            }

            return me;
        }

        /// <summary>
        /// Render this in a short descriptive style
        /// </summary>
        /// <param name="viewer">The entity looking</param>
        /// <returns>the output strings</returns>
        public string GetDescribableName(IEntity viewer)
        {
            var strength = 30 + (GetVisibleDelta(viewer) * 30);

            return GetSelf(MessagingType.Visible, strength).ToString();
        }

        internal ISensoryEvent GetSelf(MessagingType type, int strength = 30)
        {
            return new SensoryEvent()
            {
                SensoryType = type,
                Strength = strength,
                Event = new Lexica() { Phrase = Type.ToString(), Type = LexicalType.ProperNoun, Role = GrammaticalType.Subject }
            };
        }
        #endregion
    }

}
