using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.ActorBase;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Architectural.ActorBase
{
    /// <summary>
    /// Framework for entities having a gender
    /// </summary>
    public class Gender : LookupDataPartial, IGender
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Admin; } }

        /// <summary>
        /// Is this a feminine gender for gramatical purposes
        /// </summary>
        [Display(Name = "Feminine", Description = "Is this a feminine gender for gramatical purposes?")]
        [DataType(DataType.Text)]
        [Required]
        public bool Feminine { get; set; }

        /// <summary>
        /// Collective pronoun
        /// </summary>
        [StringDataIntegrity("Pluralization is blank")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Pluralization", Description = "Collective pronoun (them, they)")]
        [DataType(DataType.Text)]
        [Required]
        public string Collective { get; set; }

        /// <summary>
        /// Possessive pronoun
        /// </summary>
        [StringDataIntegrity("Possessive Pronoun is blank")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Possessive Pronoun", Description = "Possessive pronoun (his, hers)")]
        [DataType(DataType.Text)]
        [Required]
        public string Possessive { get; set; }

        /// <summary>
        /// Basic pronoun
        /// </summary>
        [StringDataIntegrity("Base Pronoun is blank")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Base Pronoun", Description = "Base pronoun (he, she, it)")]
        [DataType(DataType.Text)]
        [Required]
        public string Base { get; set; }

        /// <summary>
        /// Adult generalized noun "woman", "man"
        /// </summary>
        [StringDataIntegrity("Adult generalized form is blank")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Adult generalized form", Description = "Adult generalized noun (woman, man, thing)")]
        [DataType(DataType.Text)]
        [Required]
        public string Adult { get; set; }

        /// <summary>
        /// Child generalized noun "girl", "boy"
        /// </summary>
        [StringDataIntegrity("Child generalized form is blank")]
        [StringLength(200, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [Display(Name = "Child generalized form", Description = "Child generalized noun (girl, boy, thing)")]
        [DataType(DataType.Text)]
        [Required]
        public string Child { get; set; }
    }
}
