using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Communication.Lexical
{
    [Serializable]
    public class SensoryEvent : ISensoryEvent
    {
        /// <summary>
        /// The thing happening
        /// </summary>
        [UIHint("Lexica")]
        public ILexica Event { get; set; }

        /// <summary>
        /// The perceptive strength (higher = easier to see and greater distance noticed)
        /// </summary>
        [Range(-1, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Strength", Description = "How easy is this to sense. Stronger means it can be detected more easily and from a greater distance.")]
        public int Strength { get; set; }

        /// <summary>
        /// The type of sense used to detect this
        /// </summary>
        [Display(Name = "Sensory Type", Description = "The type of sense used to 'see' this.")]
        [UIHint("EnumDropDownList")]
        public MessagingType SensoryType { get; set; }

        public SensoryEvent()
        {
            SensoryType = MessagingType.Visible;
            Strength = 30;
        }

        public SensoryEvent(ILexica happening, int strength, MessagingType sensoryType)
        {
            Event = happening;
            Strength = strength;
            SensoryType = sensoryType;
        }

        /// <summary>
        /// Use this to create a "blank" occurrence
        /// </summary>
        public SensoryEvent(MessagingType sensoryType)
        {
            SensoryType = sensoryType;
            Strength = 30;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ILexica modifier, bool passthru = false)
        {
            return Event.TryModify(modifier, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ILexica[] modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ILexica> modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ISensoryEvent modifier, bool passthru = false)
        {
            return TryModify(modifier.Event, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ISensoryEvent[] modifier)
        {
            TryModify(modifier.Select(occ => occ.Event));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ISensoryEvent> modifier)
        {
            TryModify(modifier.Select(occ => occ.Event));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = false)
        {
            return Event.TryModify(type, role, phrase, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = false)
        {
            return TryModify(modifier.Item1, modifier.Item2, modifier.Item3, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier)
        {
            Event.TryModify(modifier);
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="normalization">How much sentence splitting should be done</param>
        /// <param name="verbosity">A measure of how much flourish should be added as well as how far words get synonym-upgraded by "finesse". (0 to 100)</param>
        /// <param name="chronology">The time tensing of the sentence structure</param>
        /// <param name="perspective">The personage of the sentence structure</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public string Describe(NarrativeNormalization normalization, int verbosity, LexicalTense chronology = LexicalTense.Present,
            NarrativePerspective perspective = NarrativePerspective.SecondPerson, bool omitName = true)
        {
            return Event.Describe(normalization, verbosity, chronology, perspective, omitName);
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <returns>a sentence fragment</returns>
        public override string ToString()
        {
            return Event.ToString();
        }
    }
}
