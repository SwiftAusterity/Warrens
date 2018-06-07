using NetMud.DataStructure.SupportingClasses;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public partial class MultipleOccurrenceViewModel : LexicaViewModel
    {
        [Display(Name = "Strength")]
        public int[] Strengths { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Phrase")]
        public string[] Phrases { get; set; }

        [Display(Name = "Grammatical Role")]
        public short[] Roles { get; set; }

        [Display(Name = "Type")]
        public short[] Types { get; set; }

        public short[] LexicaModifierIterator { get; set; }

        public IOccurrence[] OccurrenceDataObjects { get; set; }
    }

    public partial class OccurrenceViewModel : LexicaViewModel
    {
        [Range(-1, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Strength")]
        public int Strength { get; set; }

        public IOccurrence OccurrenceDataObject { get; set; }
    }

    public partial class LexicaViewModel
    {
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Phrase")]
        public string Phrase { get; set; }

        [Display(Name = "Grammatical Role")]
        public short Role { get; set; }

        [Display(Name = "Type")]
        public short Type { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Phrase")]
        public string[] ModifierPhrases { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Conjunction")]
        public string[] ModifierConjunctions { get; set; }

        [Display(Name = "Grammatical Role")]
        public short[] ModifierRoles { get; set; }

        [Display(Name = "Type")]
        public short[] ModifierLexicalTypes { get; set; }

        public ILexica LexicaDataObject { get; set; }
    }
}