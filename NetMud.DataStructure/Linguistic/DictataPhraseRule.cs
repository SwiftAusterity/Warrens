using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Rules for phrase detection
    /// </summary>
    public class DictataPhraseRule
    {
        /// <summary>
        /// The words to look for to make a phrase from
        /// </summary>
        [UIHint("DictataPhraseRuleElements")]
        public HashSet<DictataPhraseRuleElement> Elements { get; set; }

        public DictataPhraseRule()
        {
            Elements = new HashSet<DictataPhraseRuleElement>()
            {
                new DictataPhraseRuleElement(LexicalType.Adjective),
                new DictataPhraseRuleElement(LexicalType.Adverb),
                new DictataPhraseRuleElement(LexicalType.Article),
                new DictataPhraseRuleElement(LexicalType.Conjunction),
                new DictataPhraseRuleElement(LexicalType.Interjection),
                new DictataPhraseRuleElement(LexicalType.Noun),
                new DictataPhraseRuleElement(LexicalType.Preposition),
                new DictataPhraseRuleElement(LexicalType.Pronoun),
                new DictataPhraseRuleElement(LexicalType.ProperNoun),
                new DictataPhraseRuleElement(LexicalType.Verb)
            };
        }
    }
}
