using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Rules for sentence construction
    /// </summary>
    public class SentenceGrammarRule
    {
        /// <summary>
        /// Affects this fragment of the sentence
        /// </summary>
        [Display(Name = "Type", Description = "Affects this fragment of the sentence.")]
        [UIHint("EnumDropDownList")]
        public GrammaticalType Fragment { get; set; }

        /// <summary>
        /// Where does the To word fit around the From word? (the from word == 0)
        /// </summary>
        [Display(Name = "Placement Order", Description = " Where in the sentence section does this fit? (starts with 0).")]
        [DataType(DataType.Text)]
        public short ModificationOrder { get; set; }

        /// <summary>
        /// Subject vs Predicate
        /// </summary>
        [Display(Name = "Subject", Description = "Does this fit in the sentence Subject (true) or Predicate (false).")]
        [UIHint("Boolean")]
        public bool SubjectPredicate { get; set; }

        /// <summary>
        /// For this sentence type
        /// </summary>
        [Display(Name = "Grammatical Role", Description = "For this sentence type specifically.")]
        [UIHint("EnumDropDownList")]
        public SentenceType Type { get; set; }

        public SentenceGrammarRule()
        {
            SubjectPredicate = false;
            ModificationOrder = 0;
            Fragment = GrammaticalType.None;
            Type = SentenceType.None;
        }

        public SentenceGrammarRule(GrammaticalType fragment, short modificationOrder, bool subjectPredicate, SentenceType type)
        {
            SubjectPredicate = subjectPredicate;
            ModificationOrder = modificationOrder;
            Fragment = fragment;
            Type = type;
        }
    }
}
