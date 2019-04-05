using NetMud.Communication.Lexical;
using NetMud.Data.Architectural;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web.Script.Serialization;
using WordNet.Net;
using WordNet.Net.Searching;
using WordNet.Net.WordNet;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// Sort of a partial class of Lexica so it can get stored more easily and work for the processor
    /// </summary>
    [Serializable]
    public class Language : ConfigData, ILanguage, IComparable<ILanguage>, IEquatable<ILanguage>, IEqualityComparer<ILanguage>
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.ReviewOnly;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Language;

        /// <summary>
        /// The code google translate uses to identify this language
        /// </summary>
        [Display(Name = "Language Code", Description = "The language code Google Translate uses to identify this language.")]
        [DataType(DataType.Text)]
        public string GoogleLanguageCode { get; set; }

        /// <summary>
        /// Languages only used for input and output translation
        /// </summary>
        [Display(Name = "UI Only", Description = "Only for use in translating the input/output, not an 'in game' language.")]
        [UIHint("Boolean")]
        public bool UIOnly { get; set; }

        /// <summary>
        /// Does this language have gendered grammar (like most latin based)
        /// </summary>
        [Display(Name = "Gendered", Description = "Does this language have gendered grammar (like most latin based).")]
        [UIHint("Boolean")]
        public bool Gendered { get; set; }

        /// <summary>
        /// Does punctuation come at the beginning of a sentence? (spanish)
        /// </summary>
        [Display(Name = "Precedent Punctuation", Description = "Does punctuation come at the beginning of a sentence? (spanish)")]
        [UIHint("Boolean")]
        public bool PrecedentPunctuation { get; set; }

        /// <summary>
        /// Does punctuation come at the end of a sentence?
        /// </summary>
        [Display(Name = "Antecendent Punctuation", Description = "Does punctuation come at the end of a sentence?")]
        [UIHint("Boolean")]
        public bool AntecendentPunctuation { get; set; }

        /// <summary>
        /// List of grammatical rules to use in sentence construction
        /// </summary>
        [UIHint("WordRules")]
        public HashSet<IWordRule> WordRules { get; set; }

        /// <summary>
        /// List of grammatical rules to use in sentence construction
        /// </summary>
        [UIHint("WordPairRules")]
        public HashSet<IWordPairRule> WordPairRules { get; set; }

        /// <summary>
        /// List of grammatical rules to use in sentence construction
        /// </summary>
        [UIHint("ContractionRules")]
        public HashSet<IContractionRule> ContractionRules { get; set; }

        /// <summary>
        /// Word transformational rules
        /// </summary>
        [UIHint("DictataTransformationRules")]
        public HashSet<IDictataTransformationRule> TransformationRules { get; set; }

        /// <summary>
        /// Rules for sentence construction
        /// </summary>
        [UIHint("SentenceRules")]
        public HashSet<SentenceGrammarRule> SentenceRules { get; set; }

        /// <summary>
        /// Rules for sentence combination
        /// </summary>
        [UIHint("SentenceComplexityRules")]
        public HashSet<SentenceComplexityRule> ComplexityRules { get; set; }

        /// <summary>
        /// Rules for phrase detection
        /// </summary>
        [UIHint("DictataPhraseRules")]
        public HashSet<DictataPhraseRule> PhraseRules { get; set; }

        /// <summary>
        /// The base needed words for a language to function
        /// </summary>
        [UIHint("BaseLanguageWords")]
        public BaseLanguageMembers BaseWords { get; set; }

        public Language()
        {
            Name = string.Empty;
            WordRules = new HashSet<IWordRule>();
            WordPairRules = new HashSet<IWordPairRule>();
            ContractionRules = new HashSet<IContractionRule>();
            SentenceRules = new HashSet<SentenceGrammarRule>();
            ComplexityRules = new HashSet<SentenceComplexityRule>();
            BaseWords = new BaseLanguageMembers();
            TransformationRules = new HashSet<IDictataTransformationRule>();
            PhraseRules = new HashSet<DictataPhraseRule>();
        }

        /// <summary>
        /// Create or modify a lexeme with no word form basis, gets tricky with best fit scenarios
        /// </summary>
        /// <param name="word">just the text of the word</param>
        /// <returns>A lexeme</returns>
        public ILexeme CreateOrModifyLexeme(string word)
        {
            ILexeme newLex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}", Name, word));

            if(newLex != null && newLex.ContainedTypes().Any())
            {
                return newLex;
            }

            bool exists = true;
            SearchSet searchSet = null;
            List<Search> results = new List<Search>();
            LexicalProcessor.WordNet.OverviewFor(word, string.Empty, ref exists, ref searchSet, results);

            //We in theory have every single word form for this word now
            if (exists && results != null)
            {
                foreach (SynonymSet synSet in results.SelectMany(result => result.senses))
                {
                    newLex = CreateOrModifyLexeme(word, LexicalProcessor.MapLexicalTypes(synSet.pos.Flag), new string[0]);
                }
            }

            return newLex;
        }

        /// <summary>
        /// Create or modify a lexeme within this language
        /// </summary>
        /// <param name="word">the word we're making</param>
        /// <returns></returns>
        public ILexeme CreateOrModifyLexeme(string word, LexicalType form, string[] semantics)
        {
            ILexeme lex = ConfigDataCache.Get<ILexeme>(string.Format("{0}_{1}", Name, word));

            if (lex == null)
            {
                lex = new Lexeme()
                {
                    Name = word,
                    Language = this
                };
            }

            if (lex.GetForm(form, semantics, false) == null)
            {
                var newDict = new Dictata()
                {
                    Name = word,
                    WordType = form,
                    Language = this,
                    Semantics = new HashSet<string>(semantics)
                };

                lex.AddNewForm(newDict);
                lex.SystemSave();
                lex.PersistToCache();
            }

            return lex;
        }

        #region Data persistence functions
        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool Save(IAccount editor, StaffRank rank)
        {
            bool result = base.Save(editor, rank);
            EnsureDictionary();
            return result;
        }

        /// <summary>
        /// Update the field data for this object to the db
        /// </summary>
        /// <returns>success status</returns>
        public override bool SystemSave()
        {
            bool result = base.SystemSave();
            EnsureDictionary();
            return result;
        }

        private void EnsureDictionary()
        {
            if (!string.IsNullOrWhiteSpace(BaseWords.ArticleDeterminant))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.ArticleDeterminant,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.ArticleDeterminant,
                            Determinant = true,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Article,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.ArticleNonDeterminant))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.ArticleNonDeterminant,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.ArticleNonDeterminant,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Article,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.Conjunction))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.Conjunction,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.Conjunction,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Conjunction,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.NeutralPronounFirstPersonPossessive))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.NeutralPronounFirstPersonPossessive,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.NeutralPronounFirstPersonPossessive,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.FirstPerson,
                            Possessive = true,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Pronoun,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.NeutralPronounFirstPersonSingular))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.NeutralPronounFirstPersonSingular,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.NeutralPronounFirstPersonSingular,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.FirstPerson,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Pronoun,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.NeutralPronounSecondPersonPlural))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.NeutralPronounSecondPersonPlural,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.NeutralPronounSecondPersonPlural,
                            Determinant = false,
                            Feminine = false,
                            Plural = true,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.SecondPerson,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Pronoun,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.NeutralPronounSecondPersonPossessive))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.NeutralPronounSecondPersonPossessive,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.NeutralPronounSecondPersonPossessive,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.SecondPerson,
                            Possessive = true,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Pronoun,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.NeutralPronounSecondPersonSingular))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.NeutralPronounSecondPersonSingular,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.NeutralPronounSecondPersonSingular,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.SecondPerson,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Pronoun,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.VerbExistentialPlural))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.VerbExistentialPlural,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.VerbExistentialPlural,
                            Determinant = false,
                            Feminine = false,
                            Plural = true,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Semantics = new HashSet<string>() { "existential" },
                            Tense = LexicalTense.Present,
                            WordType = LexicalType.Verb,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.VerbExistentialSingular))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.VerbExistentialSingular,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.VerbExistentialSingular,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.None,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Semantics = new HashSet<string>() { "existential" },
                            Tense = LexicalTense.Present,
                            WordType = LexicalType.Verb,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionAround))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionAround,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionAround,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.Around,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            WordType = LexicalType.Preposition,
                            Tense = LexicalTense.None,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionAttached))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionAttached,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionAttached,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.Attached,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionFar))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionFar,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionFar,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.Far,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionInside))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionInside,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionInside,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.InsideOf,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionNear))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionNear,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionNear,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.Near,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionOn))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionOn,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionOn,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.On,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }

            if (!string.IsNullOrWhiteSpace(BaseWords.PrepositionOf))
            {
                LexicalProcessor.VerifyLexeme(new Lexeme()
                {
                    Name = BaseWords.PrepositionOf,
                    Language = this,
                    WordForms = new HashSet<IDictata>() {
                        new Dictata()
                        {
                            Name = BaseWords.PrepositionOf,
                            Determinant = false,
                            Feminine = false,
                            Plural = false,
                            Positional = LexicalPosition.PartOf,
                            Perspective = NarrativePerspective.None,
                            Possessive = false,
                            Tense = LexicalTense.None,
                            WordType = LexicalType.Preposition,
                            Language = this
                        }
                    }
                });
            }
        }
        #endregion

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        public override IDictionary<string, string> SignificantDetails()
        {
            IDictionary<string, string> returnList = base.SignificantDetails();

            returnList.Add("UIOnly", UIOnly.ToString());
            returnList.Add("Code", GoogleLanguageCode);
            returnList.Add("Precedent Punctuation", PrecedentPunctuation.ToString());
            returnList.Add("Antecendent Punctuation", AntecendentPunctuation.ToString());
            returnList.Add("Gendered", Gendered.ToString());

            return returnList;
        }

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            try
            {
                return CompareTo(other as ILanguage);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return -99;
        }

        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILanguage other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return 1;
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILanguage other)
        {
            if (other != default(ILanguage))
            {
                try
                {
                    return other.Name.Equals(Name, StringComparison.InvariantCultureIgnoreCase);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILanguage x, ILanguage y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILanguage obj)
        {
            return obj.GetType().GetHashCode() + obj.Name.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + Name.GetHashCode();
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
