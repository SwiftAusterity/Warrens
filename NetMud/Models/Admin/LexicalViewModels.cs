using NetMud.Authentication;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Models.Admin
{
    public partial class OccurrenceViewModel : LexicaViewModel, BaseViewModel
    {
        public ApplicationUser authedUser { get; set; }

        [Range(-1, 1000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [DataType(DataType.Text)]
        [Display(Name = "Strength", Description = "How easy is this to sense. Stronger means it can be detected more easily and from a greater distance.")]
        public int Strength { get; set; }

        [Display(Name = "Sensory Type", Description = "The type of sense used to 'see' this.")]
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
        [Display(Name = "Phrase", Description = "The base phrase.")]
        public string Phrase { get; set; }

        [Display(Name = "Grammatical Role", Description = "The role this phrase plays in a sentence.")]
        public short Role { get; set; }

        [Display(Name = "Type", Description = "The type of word this is.")]
        public short Type { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Phrase", Description = "A modifying phrase of the base word.")]
        public string[] ModifierPhrases { get; set; }

        [Display(Name = "Grammatical Role", Description = "The role this modifier plays in a sentence.")]
        public short[] ModifierRoles { get; set; }

        [Display(Name = "Type", Description = "The type of word this is.")]
        public short[] ModifierLexicalTypes { get; set; }

        public ILexica LexicaDataObject { get; set; }
    }
}