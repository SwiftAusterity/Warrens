using NetMud.Communication.Lexical;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Utility;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Web.Script.Serialization;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    [Serializable]
    public class Lexica : ILexica
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        [Display(Name = "Grammatical Role", Description = "The role this phrase plays in a sentence.")]
        [UIHint("EnumDropDownList")]
        public GrammaticalType Role { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        [Display(Name = "Type", Description = "The type of word this is.")]
        [UIHint("EnumDropDownList")]
        public LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        [StringLength(100, ErrorMessage = "The {0} must be between {2} and {1} characters long.", MinimumLength = 2)]
        [DataType(DataType.Text)]
        [Display(Name = "Phrase", Description = "The base phrase.")]
        public string Phrase { get; set; }

        /// <summary>
        /// Modifiers for this lexica
        /// </summary>
        [Display(Name = "Modifier", Description = "A modifying phrase of the base word.")]
        [UIHint("LexicalModifiers")]
        public HashSet<ILexica> Modifiers { get; set; }

        /// <summary>
        /// The context for this, which gets passed downards to anything modifying it
        /// </summary>
        [JsonIgnore]
        [ScriptIgnore]
        public LexicalContext Context { get; set; }

        public Lexica()
        {
            Modifiers = new HashSet<ILexica>();
            Context = new LexicalContext(null);
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase, LexicalContext context)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
            Context = context.Clone();
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase, IEntity origin, IEntity observer)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
            Context = BuildContext(origin, observer);
        }

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        public IDictata GetDictata()
        {
            return ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), string.Format("{0}_{1}_{2}", Context?.Language?.Name, Type.ToString(), Phrase), ConfigDataType.Dictionary));
        }

        /// <summary>
        /// Generate a new dictata from this
        /// </summary>
        /// <returns></returns>
        public IDictata GenerateDictata()
        {
            return LexicalProcessor.VerifyDictata(new Dictata(this));
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(ILexica modifier, bool passthru = false)
        {
            if (Modifiers == null)
            {
                Modifiers = new HashSet<ILexica>();
            }

            if (!Modifiers.Contains(modifier))
            {
                if (modifier.Context == null)
                {
                    modifier.Context = Context.Clone();
                }
                else
                {
                    //Sentence must maintain the same observer, language, tense and personage
                    modifier.Context.Language = Context.Language;
                    modifier.Context.Tense = Context.Tense;
                    modifier.Context.Perspective = Context.Perspective;
                    modifier.Context.Observer = Context.Observer;
                }

                Modifiers.Add(modifier);
            }

            return passthru ? this : modifier;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ILexica[] modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            foreach (ILexica mod in modifiers)
            {
                TryModify(mod);
            }
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ILexica> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            foreach (ILexica mod in modifiers)
            {
                TryModify(mod);
            }
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(HashSet<ILexica> modifiers)
        {
            if (modifiers == null)
            {
                return;
            }

            foreach (ILexica mod in modifiers)
            {
                TryModify(mod);
            }
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = false)
        {
            ILexica modifier = new Lexica(type, role, phrase, Context);

            modifier = TryModify(modifier);

            return passthru ? this : modifier;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = false)
        {
            return TryModify(modifier.Item1, modifier.Item2, modifier.Item3, passthru);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier)
        {
            foreach (Tuple<LexicalType, GrammaticalType, string> mod in modifier)
            {
                TryModify(mod);
            }
        }

        /// <summary>
        /// Unpacks this applying language rules to expand and add articles/verbs where needed
        /// </summary>
        /// <param name="overridingContext">The full lexical context</param>
        /// <returns>A long description</returns>
        public IEnumerable<ILexica> Unpack(MessagingType sensoryType, int strength, LexicalContext overridingContext = null)
        {
            if (overridingContext != null)
            {
                //Sentence must maintain the same language, tense and personage
                Context.Language = overridingContext.Language;
                Context.Tense = overridingContext.Tense;
                Context.Perspective = overridingContext.Perspective;
            }

            var lexList = new List<ILexica>();

            var obfuscationLevel = Math.Max(0, Math.Min(100, 30 - strength));
            var newLex = Mutate(sensoryType, strength, obfuscationLevel);

            //Placement ordering
            var modifierList = new List<Tuple<ILexica, int>>
            {
                new Tuple<ILexica, int>(newLex, 0)
            };
            foreach (var modifierPair in Modifiers.GroupBy(lexi => new { lexi.Role, lexi.Type }))
            {
                var rule = Context.Language.Rules.OrderByDescending(rul => rul.RuleSpecificity())
                                                 .FirstOrDefault(rul => rul.Matches(this, modifierPair.Key.Role, modifierPair.Key.Type));

                if (rule != null)
                {
                    foreach (var modifier in modifierPair)
                    {
                        modifierList.Add(new Tuple<ILexica, int>(modifier, rule.ModificationOrder));
                    }
                }
            }

            newLex.Modifiers = new HashSet<ILexica>();

            return modifierList.OrderBy(tup => tup.Item2).Select(tup => tup.Item1);
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <returns>a sentence fragment</returns>
        public string Describe()
        {
            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(Phrase))
            {
                return string.Empty;
            }

            //up-caps all the proper nouns
            if (Type == LexicalType.ProperNoun)
            {
                Phrase = Phrase.ProperCaps();
            }
            else
            {
                Phrase = Phrase.ToLower();
            }

            return Phrase;
        }

        /// <summary>
        /// Alter the lex entirely including all of its sublex
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <param name="obfuscationLevel">% level of obfuscating this thing (0 to 100).</param>
        /// <returns>the new lex</returns>
        private ILexica Mutate(MessagingType sensoryType, int strength, int obfuscationLevel = 0)
        {
            var rand = new Random();
            var dict = GetDictata();
            if (Type != LexicalType.ProperNoun && dict != null && (Context.Severity + Context.Elegance + Context.Quality > 0 || Context.Language != dict.Language))
            {
                var newDict = Thesaurus.GetSynonym(dict, Context);

                Phrase = newDict.Name;
            }

            if (obfuscationLevel < 0 || obfuscationLevel > rand.Next(0, 100))
            {
                var lex = RunObscura(sensoryType, Context.Observer, obfuscationLevel >= 100);
                lex.Modifiers.RemoveWhere(mod => mod.Role == GrammaticalType.Descriptive);

                return lex;
            }

            return this;
        }

        /// <summary>
        /// Make a sentence out of this
        /// </summary>
        /// <param name="type">the sentence type</param>
        /// <returns>the sentence</returns>
        public ILexicalSentence MakeSentence(SentenceType type)
        {
            return new LexicalSentence(this) { Type = type };
        }

        /// <summary>
        /// Build out the context object
        /// </summary>
        /// <param name="entity">the subject</param>
        private LexicalContext BuildContext(IEntity entity, IEntity observer)
        {
            var context = new LexicalContext(observer);

            var specific = true;
            var entityLocation = entity?.CurrentLocation?.CurrentLocation();
            if (entityLocation != null)
            {
                var type = entity.GetType();

                //We're looking for more things with the same template id (ie based on the same thing, like more than one wolf or sword)
                if (type == typeof(IInanimate))
                {
                    specific = !entityLocation.GetContents<IInanimate>().Any(item => item != entity && item.TemplateId == entity.TemplateId);
                }
                else if (type == typeof(INonPlayerCharacter))
                {
                    specific = !entityLocation.GetContents<INonPlayerCharacter>().Any(item => item != entity && item.TemplateId == entity.TemplateId);
                    context.GenderForm = ((INonPlayerCharacter)entity).Gender;
                }
                else if (type == typeof(IPlayer))
                {
                    context.GenderForm = ((IPlayer)entity).Gender;
                }
            }

            context.Determinant = specific;

            return context;
        }

        private ILexica RunObscura(MessagingType sensoryType, IEntity observer, bool over)
        {
            ILexica message = null;

            var context = new LexicalContext(observer)
            {
                Determinant = true,
                Perspective = NarrativePerspective.FirstPerson,
                Position = LexicalPosition.Around
            };

            switch (sensoryType)
            {
                case MessagingType.Audible:
                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "sounds", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "soft", context));

                    }
                    else
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "hear", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "sounds", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "loud", context));
                    }
                    break;
                case MessagingType.Olefactory:
                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "something", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "subtle", context));

                    }
                    else
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "smell", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "something", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "pungent", context));
                    }
                    break;
                case MessagingType.Psychic:
                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "sense", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "presence", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "vague", context));

                    }
                    else
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "sense", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "presence", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "disturbing", context));
                    }
                    break;
                case MessagingType.Tactile:
                    context.Position = LexicalPosition.Attached;
                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "brushes", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "skin", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "lightly", context));

                    }
                    else
                    {
                        context.Elegance = -5;
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "rubs", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Pronoun, GrammaticalType.Subject, "you", context));
                    }
                    break;
                case MessagingType.Taste:
                    context.Position = LexicalPosition.InsideOf;

                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "taste", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "something", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "subtle", context));

                    }
                    else
                    {
                        context.Elegance = -5;
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "taste", observer, observer)
                                                        .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "something", context))
                                                        .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "offensive", context));
                    }
                    break;
                case MessagingType.Visible:
                    context.Plural = true;

                    if (!over)
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", observer, observer)
                                                .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "shadows", context));
                    }
                    else
                    {
                        message = new Lexica(LexicalType.Verb, GrammaticalType.Verb, "see", observer, observer)
                                                .TryModify(new Lexica(LexicalType.Noun, GrammaticalType.Subject, "lights", context))
                                                .TryModify(new Lexica(LexicalType.Adjective, GrammaticalType.Descriptive, "blinding", context));
                    }
                    break;
            }

            return message;
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
                return CompareTo(other as ILexica);
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
        public int CompareTo(ILexica other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                    {
                        return -1;
                    }

                    if (other.Phrase.Equals(Phrase) && other.Type == Type)
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
        public bool Equals(ILexica other)
        {
            if (other != default(ILexica))
            {
                try
                {
                    return other.Phrase.Equals(Phrase) && other.Type == Type;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }
        #endregion
    }
}
