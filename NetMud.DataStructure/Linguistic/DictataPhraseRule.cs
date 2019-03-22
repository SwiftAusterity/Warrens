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
            Elements = new HashSet<DictataPhraseRuleElement>();
        }
    }
}
