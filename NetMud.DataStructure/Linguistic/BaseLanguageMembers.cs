using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// The base needed word types for a language to function
    /// </summary>
    public class BaseLanguageMembers
    {
        #region Neutral Pronoun second-person singular/plural/possessive
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Plural", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonPlural { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun Second Person Possessive", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounSecondPersonPossessive { get; set; }
        #endregion

        #region Neutral pronoun first-person singular/possessive
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun First Person Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounFirstPersonSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Neutral Pronoun First Person Possessive", Description = "A base word, the name says it all.")]
        [Required]
        public string NeutralPronounFirstPersonPossessive { get; set; }
        #endregion

        #region Article determinant/non
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Article Determinant", Description = "A base word, the name says it all.")]
        [Required]
        public string ArticleDeterminant { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Article Non-Determinant", Description = "A base word, the name says it all.")]
        [Required]
        public string ArticleNonDeterminant { get; set; }
        #endregion

        #region Conjunction
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Conjunction", Description = "A base word, the name says it all.")]
        [Required]
        public string Conjunction { get; set; }
        #endregion

        #region Verb existential singular/plural
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Existential Singular", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbExistentialSingular { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Existential Plural", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbExistentialPlural { get; set; }
        #endregion

        #region Verb articles positional far, near, attached, on, inside, around
        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional Far", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalFar { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional Near", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalNear { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional Attached", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalAttached { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional On", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalOn { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional Inside", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalInside { get; set; }

        /// <summary>
        /// A base word, the name says it all
        /// </summary>
        [DataType(DataType.Text)]
        [Display(Name = "Verb Article Positional Around", Description = "A base word, the name says it all.")]
        [Required]
        public string VerbArticlePositionalAround { get; set; }
        #endregion
    }
}
