using NetMud.Authentication;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public partial class OccurrenceViewModel : LexicaViewModel, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Range(-1, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Strength")]
        public int Strength { get; set; }

        [Display(Name = "Sensory Type")]
        public short SensoryType { get; set; }

        public string AdminTypeName { get; set; }
        public string DataUnitTitle { get; set; }

        public IOccurrence OccurrenceDataObject { get; set; }
        public IEntityBackingData DataObject { get; set; }
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

        [Display(Name = "Grammatical Role")]
        public short[] ModifierRoles { get; set; }

        [Display(Name = "Type")]
        public short[] ModifierLexicalTypes { get; set; }

        public ILexica LexicaDataObject { get; set; }
    }
}