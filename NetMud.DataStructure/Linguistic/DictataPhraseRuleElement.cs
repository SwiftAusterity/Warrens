using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// The words to look for to make a phrase from
    /// </summary>
    public class DictataPhraseRuleElement
    {
        /// <summary>
        /// The phrase must have N of this type of word
        /// </summary>
        [Display(Name = "Word Type", Description = " The phrase must have N of this type of word.")]
        [UIHint("EnumDropDownList")]
        public LexicalType WordType { get; set; }

        /// <summary>
        /// How many of this type of word should this have
        /// </summary>
        [Display(Name = "Minimum Count", Description = "How many of this type of word should this have.")]
        [DataType(DataType.Text)]
        public short MinimumNumber { get; set; }

        /// <summary>
        /// Is this the primary word type (for pulling context)
        /// </summary>
        [Display(Name = "Primary", Description = "Is this the primary word type (for pulling context).")]
        [UIHint("Boolean")]
        public bool Primary { get; set; }

        public DictataPhraseRuleElement()
        {
            MinimumNumber = 0;
            WordType = LexicalType.None;
            Primary = false;
        }
    }
}
