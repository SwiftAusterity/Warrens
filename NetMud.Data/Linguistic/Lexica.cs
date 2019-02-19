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
            Context = new LexicalContext();
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase, LexicalContext context)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
            Context = context;
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase, IEntity origin)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
            Context = BuildContext(origin);
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
            if(Modifiers == null)
            {
                Modifiers = new HashSet<ILexica>();
            }

            if (!Modifiers.Contains(modifier))
            {
                if(modifier.Context == null)
                {
                    modifier.Context = Context;
                }
                else
                {
                    //Sentence must maintain the same language, tense and personage
                    modifier.Context.Language = Context.Language;
                    modifier.Context.Tense = Context.Tense;
                    modifier.Context.Perspective = Context.Perspective;
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
        /// Create a narrative description from this
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public string Unpack(LexicalContext overridingContext = null, bool omitName = false)
        {
            if (overridingContext != null)
            {
                //Sentence must maintain the same language, tense and personage
                Context.Language = overridingContext.Language;
                Context.Tense = overridingContext.Tense;
                Context.Perspective = overridingContext.Perspective;
            }

            IEnumerable<LexicalSentence> sentences = GetSentences(omitName);

            return string.Join(" ", sentences.Select(sent => sent.Describe())).Trim();
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <returns>a sentence fragment</returns>
        public string Describe()
        {
            var lex = Context.Language == null && (Context.Severity + Context.Elegance + Context.Quality == 0)
                    ? this
                    : Mutate();

            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(lex.Phrase))
            {
                return string.Empty;
            }

            //Language rules engine, default to base language if we have an empty language
            if (Context.Language == null || Context.Language?.Rules?.Count == 0)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                Context.Language = globalConfig.BaseLanguage;
            }

            //up-caps all the proper nouns
            if (lex.Type == LexicalType.ProperNoun)
            {
                lex.Phrase = lex.Phrase.ProperCaps();
            }
            else
            {
                lex.Phrase = lex.Phrase.ToLower();
            }

            //Contractive rules
            var lexDict = lex.GetDictata();
            foreach (var rule in Context.Language.ContractionRules.Where(rul => rul.First == lexDict || rul.Second == lexDict))
            {
                if(!lex.Modifiers.Any(mod => mod.GetDictata() == rule.First || mod.GetDictata() == rule.Second))
                {
                    continue;
                }

                lex.Modifiers.RemoveWhere(mod => mod.GetDictata() == rule.First || mod.GetDictata() == rule.Second);

                lex.Phrase = rule.Contraction.Name;
            }

            //Listable pass rules
            var modifierList = new List<Tuple<ILexica[], int>>();
            foreach (var modifierPair in lex.Modifiers.GroupBy(lexi => new { lexi.Role, lexi.Type }))
            {
                var rule = Context.Language.Rules.OrderByDescending(rul => rul.RuleSpecificity())
                                                 .FirstOrDefault(rul => rul.Matches(lex, modifierPair.Key.Role, modifierPair.Key.Type));

                if (rule != null)
                {
                    if (rule.Listable)
                    {
                        modifierList.Add(new Tuple<ILexica[], int>(modifierPair.ToArray(), rule.ModificationOrder));
                    }
                    else
                    {
                        foreach (var modifier in modifierPair)
                        {
                            modifierList.Add(new Tuple<ILexica[], int>(new ILexica[] { modifier }, rule.ModificationOrder));
                        }
                    }
                }
            }

            modifierList.Add(new Tuple<ILexica[], int>(new ILexica[] { lex }, 0));

            StringBuilder sb = new StringBuilder();
            foreach (var grouping in modifierList.OrderBy(mod => mod.Item2))
            {
                if (grouping.Item1.Length > 1)
                {
                    sb.Append(grouping.Item1.Select(lexi => lexi.Describe()).CommaList(RenderUtility.SplitListType.CommaWithAnd));
                }
                else
                {
                    ILexica item = grouping.Item1[0];

                    if (item == lex)
                    {
                        sb.Append(item.Phrase + " ");
                    }
                    else
                    {
                        sb.Append(item.Describe() + " ");
                    }
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Alter the lex entirely including all of its sublex
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <param name="obfuscationLevel">% level of obfuscating this thing (0 to 100).</param>
        /// <returns>the new lex</returns>
        public ILexica Mutate(int obfuscationLevel = 0)
        {
            Lexica newLexica = new Lexica(Type, Role, Phrase, Context);

            var dict = GetDictata();

            if (Type != LexicalType.ProperNoun && dict != null && (Context.Severity + Context.Elegance + Context.Quality > 0 || Context.Language != dict.Language))
            {
                var newDict = Thesaurus.GetSynonym(dict, Context);

                newLexica = new Lexica(Type, Role, newDict.Name, Context);
            }

            newLexica.TryModify(Modifiers);

            return newLexica;
        }

        private IEnumerable<LexicalSentence> GetSentences(bool omitName)
        {
            ILexica me;

            List<LexicalSentence> sentences = new List<LexicalSentence>();

            if (omitName)
            {
                var pronounContext = Context;
                pronounContext.Perspective = NarrativePerspective.SecondPerson;
                pronounContext.Position = LexicalPosition.None;
                pronounContext.Tense = LexicalTense.None;
                pronounContext.Determinant = false;
                pronounContext.Semantics = new HashSet<string>();

                var pronoun = Thesaurus.GetWord(pronounContext, LexicalType.Pronoun);
                me = new Lexica(pronoun.WordType, Role, pronoun.Name, Context);
                me.TryModify(Modifiers.Where(mod => mod != null && mod.Role != GrammaticalType.Subject));
            }
            else
            {
                me = new Lexica(Type, Role, Phrase, Context);
                me.TryModify(Modifiers.Where(mod => mod != null && mod.Role != GrammaticalType.Subject));
            }

            List<ILexica> subjects = new List<ILexica>
            {
                me
            };
            subjects.AddRange(Modifiers.Where(mod => mod != null && mod.Role == GrammaticalType.Subject));

            foreach (ILexica subject in subjects)
            {
                //Language rules engine, default to base language if we have an empty language
                if (subject.Context.Language == null || subject.Context.Language?.Rules?.Count == 0)
                {
                    IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                    subject.Context.Language = globalConfig.BaseLanguage;
                }

                var newLex = subject;
                //This is to catch directly described entities, we have to add a verb to it for it to make sense.
                if (subject.Modifiers.Any() && !subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                {
                    Lexica newSubject = new Lexica(subject.Type, subject.Role, subject.Phrase, subject.Context);
                    newSubject.TryModify(subject.Modifiers);

                    var verbContext = subject.Context;
                    verbContext.Semantics = new HashSet<string> { "existential" };
                    verbContext.Determinant = false;
                    var verb = Thesaurus.GetWord(subject.Context, LexicalType.Verb);

                    var verbLex = new Lexica(LexicalType.Verb, GrammaticalType.Verb, verb.Name, verbContext);
                    verbLex.TryModify(newSubject.Modifiers);

                    newSubject.Modifiers = null;
                    newSubject.TryModify(verbLex);

                    newLex = newSubject;
                }

                if (newLex.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                {
                    sentences.Add(new LexicalSentence(newLex) { Type = SentenceType.Partial });

                    //fragment sentences
                    foreach (ILexica subLex in newLex.Modifiers.Where(mod => mod.Role == GrammaticalType.Subject))
                    {
                        sentences.Add(new LexicalSentence(subLex) { Type = SentenceType.Statement });
                    }
                }
                else
                {
                    sentences.Add(new LexicalSentence(newLex) { Type = SentenceType.Statement });
                }
            }

            return sentences;
        }

        /// <summary>
        /// Build out the context object
        /// </summary>
        /// <param name="entity">the subject</param>
        private LexicalContext BuildContext(IEntity entity)
        {
            var context = new LexicalContext();

            var specific = true;
            var entityLocation = entity.CurrentLocation?.CurrentLocation();
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
