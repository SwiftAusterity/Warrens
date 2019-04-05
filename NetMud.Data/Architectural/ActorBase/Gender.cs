using NetMud.Communication.Lexical;
using NetMud.Data.Architectural.DataIntegrity;
using NetMud.Data.Linguistic;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
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

        #region Data persistence functions
        /// <summary>
        /// Add it to the cache and save it to the file system
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IKeyedData Create(IAccount creator, StaffRank rank)
        {
            EnsureDictionary();
            return base.Create(creator, rank);
        }

        /// <summary>
        /// Add it to the cache and save it to the file system made by SYSTEM
        /// </summary>
        /// <returns>the object with Id and other db fields set</returns>
        public override IKeyedData SystemCreate()
        {
            EnsureDictionary();
            return base.SystemCreate();
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save(IAccount editor, StaffRank rank)
        {
            EnsureDictionary();
            return base.Save(editor, rank);
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemSave()
        {
            EnsureDictionary();
            return base.SystemSave();
        }

        private void EnsureDictionary()
        {
            Lexeme collective = new Lexeme()
            {
                Name = Collective,
                WordForms = new IDictata[] {
                    new Dictata()
                    {
                        Name = Collective,
                        Determinant = false,
                        Feminine = Feminine,
                        Plural = true,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Possessive = false,
                        Tense = LexicalTense.None,
                        Semantics = new HashSet<string>() { "gender" },
                        WordType = LexicalType.Pronoun
                    }
                }
            };

            Lexeme possessive = new Lexeme()
            {
                Name = Possessive,
                WordForms = new IDictata[] {
                    new Dictata()
                    {
                        Name = Possessive,
                        Determinant = false,
                        Feminine = Feminine,
                        Plural = false,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Possessive = true,
                        Tense = LexicalTense.None,
                        Semantics = new HashSet<string>() { "gender" },
                        WordType = LexicalType.Pronoun
                    }
                }
            };

            Lexeme baseWord = new Lexeme()
            {
                Name = Base,
                WordForms = new IDictata[] {
                    new Dictata()
                    {
                        Name = Base,
                        Determinant = false,
                        Feminine = Feminine,
                        Plural = false,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Possessive = false,
                        Tense = LexicalTense.None,
                        Semantics = new HashSet<string>() { "gender" },
                        WordType = LexicalType.Pronoun
                    }
                }
            };

            Lexeme adult = new Lexeme()
            {
                Name = Adult,
                WordForms = new IDictata[] {
                    new Dictata()
                    {
                        Name = Adult,
                        Determinant = false,
                        Feminine = Feminine,
                        Plural = false,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Semantics = new HashSet<string>() { "adult", "gender" },
                        Possessive = false,
                        Tense = LexicalTense.None,
                        WordType = LexicalType.Noun
                    }
                }
            };

            Lexeme child = new Lexeme()
            {
                Name = Child,
                WordForms = new IDictata[] {
                    new Dictata()
                    {
                        Name = Child,
                        Determinant = false,
                        Feminine = Feminine,
                        Plural = false,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Semantics = new HashSet<string>() { "child", "gender" },
                        Possessive = false,
                        Tense = LexicalTense.None,
                        WordType = LexicalType.Noun
                    }
                }
            };

            LexicalProcessor.VerifyLexeme(child);
            LexicalProcessor.VerifyLexeme(adult);
            LexicalProcessor.VerifyLexeme(baseWord);
            LexicalProcessor.VerifyLexeme(possessive);
            LexicalProcessor.VerifyLexeme(collective);
        }
        #endregion
    }
}
