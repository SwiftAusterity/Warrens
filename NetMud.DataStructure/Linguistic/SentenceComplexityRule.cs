using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Defines rules that allow sentences to combine
    /// </summary>
    public class SentenceComplexityRule
    {
        /// <summary>
        /// At what level of elegance is this valid for
        /// </summary>
        [Display(Name = "Elegance Threshold", Description = "At what level of elegance is this valid for.")]
        [DataType(DataType.Text)]
        [Required]
        public int EleganceThreshold { get; set; }

        /// <summary>
        /// If the two pivots should match or if they should not match
        /// </summary>
        [Display(Name = "Pivot Match", Description = "If the two pivots should match (while the rest doesn't) or if they should not match (while the rest does).")]
        [UIHint("Boolean")]
        public bool PivotMatch { get; set; }

        /// <summary>
        /// When the word that's in this role for the sentence matches the other pivot
        /// </summary>
        [Display(Name = "Subject Pivot", Description = "When the word that's in this role for the sentence matches the other pivot. Becomes the subject of the new sentence.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public GrammaticalType SubjectPivot { get; set; }

        /// <summary>
        /// When the word that's in this role for the sentence matches the other pivot
        /// </summary>
        [Display(Name = "Predicate Pivot", Description = "When the word that's in this role for the sentence matches the other pivot. Becomes the predicate of the new sentence.")]
        [UIHint("EnumDropDownList")]
        [Required]
        public GrammaticalType PredicatePivot { get; set; }

        /// <summary>
        /// Should the sensory type of the two sentences match. (false = doesn't care)
        /// </summary>
        [Display(Name = "Sensory Match", Description = "Should the sensory type of the two sentences match. (false = doesn't care)")]
        [UIHint("Boolean")]
        public bool SensoryMatch { get; set; }

        public SentenceComplexityRule()
        {
            SensoryMatch = true;
            PivotMatch = true;
            EleganceThreshold = 1;
            SubjectPivot = GrammaticalType.None;
            PredicatePivot = GrammaticalType.None;
        }

        /// <summary>
        /// Whether these two sentences match this rule
        /// </summary>
        /// <param name="first">The first sentence (optimally which one we want the subject from)</param>
        /// <param name="second">The second sentence (optimally which one we want the predicate from)</param>
        /// <param name="elegance">The elegance threshold to meet</param>
        /// <returns>-1 = subject/subject match, -2 = predicate/predicate match, 0 = no match, 1 = subject/predicate match, 2 = predicate/subject match</returns>
        public short MatchesRule(ILexicalSentence first, ILexicalSentence second, int elegance)
        {
            if (EleganceThreshold > elegance
                || (SensoryMatch && first.SensoryType != second.SensoryType)
                || first.Language != second.Language
                || first.Type != second.Type
                )
            {
                return 0;
            }

            Tuple<ISensoryEvent, short> firstSubjectPivot = first.Subject.FirstOrDefault(word => word.Item1.Event.Role == SubjectPivot);
            Tuple<ISensoryEvent, short> firstPredicatePivot = first.Predicate.FirstOrDefault(word => word.Item1.Event.Role == PredicatePivot);
            Tuple<ISensoryEvent, short> secondSubjectPivot = second.Subject.FirstOrDefault(word => word.Item1.Event.Role == SubjectPivot);
            Tuple<ISensoryEvent, short> secondPredicatePivot = second.Predicate.FirstOrDefault(word => word.Item1.Event.Role == PredicatePivot);

            if (CheckPivotValidity(firstSubjectPivot?.Item1?.Event, secondSubjectPivot?.Item1?.Event)
                && CheckNonPivotValidity(first, second))
            {
                return -1;
            }

            if (CheckPivotValidity(firstPredicatePivot?.Item1?.Event, secondPredicatePivot?.Item1?.Event)
                && CheckNonPivotValidity(first, second))
            {
                return -2;
            }

            if (CheckPivotValidity(firstSubjectPivot?.Item1?.Event, secondPredicatePivot?.Item1?.Event)
                && CheckNonPivotValidity(first, second))
            {
                return 1;
            }

            if (CheckPivotValidity(firstPredicatePivot?.Item1?.Event, secondSubjectPivot?.Item1?.Event)
                && CheckNonPivotValidity(first, second))
            {
                return 2;
            }

            return 0;
        }

        private bool CheckPivotValidity(ILexica first, ILexica second)
        {
            return first?.Context != null
                && second?.Context != null
                && first.Context.Position == second.Context.Position
                && first.Context.Tense == second.Context.Tense
                && PivotMatch == first.Phrase.Equals(second.Phrase, StringComparison.InvariantCultureIgnoreCase);
        }

        private bool CheckNonPivotValidity(ILexicalSentence first, ILexicalSentence second)
        {
            GrammaticalType[] significantTypes = new GrammaticalType[] { GrammaticalType.Verb, GrammaticalType.ConjugatedVerb, GrammaticalType.Subject, GrammaticalType.DirectObject };
            global::System.Collections.Generic.IEnumerable<Tuple<ISensoryEvent, short>> firstChecks = first.Predicate.Where(word => word.Item1?.Event != null && word.Item1.Event.Role != SubjectPivot && significantTypes.Contains(word.Item1.Event.Role));
            global::System.Collections.Generic.IEnumerable<Tuple<ISensoryEvent, short>> secondChecks = second.Subject.Where(word => word.Item1?.Event != null && word.Item1.Event.Role != PredicatePivot && significantTypes.Contains(word.Item1.Event.Role));

            global::System.Collections.Generic.IEnumerable<Tuple<ISensoryEvent, short>> firstMatches = second.Predicate.Where(word => word.Item1?.Event != null && word.Item1.Event.Role != SubjectPivot && significantTypes.Contains(word.Item1.Event.Role));
            global::System.Collections.Generic.IEnumerable<Tuple<ISensoryEvent, short>> secondMatches = first.Subject.Where(word => word.Item1?.Event != null && word.Item1.Event.Role != PredicatePivot && significantTypes.Contains(word.Item1.Event.Role));

            bool returnValue = !PivotMatch;
            if (firstChecks.Any())
            {
                returnValue = returnValue
                    && !PivotMatch == firstChecks.Any(wordPair => firstMatches.Any(matchPair => matchPair.Item1.Event.Role == wordPair.Item1.Event.Role
                                                && matchPair.Item1.Event.Phrase.Equals(wordPair.Item1.Event.Phrase, StringComparison.InvariantCultureIgnoreCase)));
            }

            if (secondChecks.Any())
            {
                returnValue = returnValue
                    && !PivotMatch == secondChecks.Any(wordPair => secondMatches.Any(matchPair => matchPair.Item1.Event.Role == wordPair.Item1.Event.Role
                                                 && matchPair.Item1.Event.Phrase.Equals(wordPair.Item1.Event.Phrase, StringComparison.InvariantCultureIgnoreCase)));
            }

            return returnValue;
        }
    }
}
