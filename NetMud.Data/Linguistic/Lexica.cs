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
            return ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), string.Format("{0}_{1}", Type.ToString(), Phrase), ConfigDataType.Dictionary));
        }

        /// <summary>
        /// Generate a new dictata from this
        /// </summary>
        /// <returns></returns>
        public bool GenerateDictata()
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
            if (!Modifiers.Contains(modifier))
            {
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
            Lexica modifier = new Lexica(type, role, phrase, Context);
            if (!Modifiers.Contains(modifier))
            {
                Modifiers.Add(modifier);
            }

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
        public string Unpack(LexicalContext context, bool omitName = true)
        {
            IEnumerable<LexicalSentence> sentences = GetSentences(omitName);

            return string.Join(" ", sentences.Select(sent => sent.Describe())).Trim();
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <returns>a sentence fragment</returns>
        public string Describe(LexicalContext context)
        {
            var lex = context.Language == null && (context.Severity + context.Elegance + context.Quality == 0)
                    ? this
                    : Mutate(context);

            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(lex.Phrase))
            {
                return string.Empty;
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

            IEnumerable<ILexica> adjectives = lex.Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive);

            //Language rules engine, default to base language if we have an empty language
            if (context.Language.Rules?.Count == 0)
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                context.Language = globalConfig.BaseLanguage;
            }

            //solitaire rules
            foreach (var rule in context.Language.Rules.Where(rul => rul.ToRole == GrammaticalType.None && rul.ToType == LexicalType.None &&
                                                        (rul.SpecificWord == lex.GetDictata() || (rul.FromRole == lex.Role && rul.FromType == lex.Type))))
            {
                if (rule.NeedsArticle && !lex.Modifiers.Any(mod => mod.Type == LexicalType.Article))
                {
                    var articleContext = context;
                    articleContext.Determinant = !context.Plural;
                    var article = Thesaurus.GetWord(context, LexicalType.Article);

                    lex.TryModify(LexicalType.Article, GrammaticalType.Descriptive, article.Name);
                }
            }

            //modifier rules
            var modifierList = new List<Tuple<ILexica, bool, int>>();
            foreach (var modifier in lex.Modifiers)
            {
                var rule = context.Language.Rules.FirstOrDefault(rul => rul.ToRole == modifier.Role && rul.ToType == modifier.Type &&
                                                                (rul.SpecificWord == lex.GetDictata() || (rul.FromRole == lex.Role && rul.FromType == lex.Type)));

                if (rule != null)
                {
                    modifierList.Add(new Tuple<ILexica, bool, int>(modifier, rule.Listable, rule.ModificationOrder));
                }
            }

            modifierList.Add(new Tuple<ILexica, bool, int>(lex, false, 0));

            StringBuilder sb = new StringBuilder();
            foreach (var grouping in modifierList.OrderBy(mod => mod.Item3))
            {
                if (grouping.Item1 == lex)
                {
                    sb.Append(grouping.Item1.Phrase + " ");
                }
                else
                {
                    sb.Append(grouping.Item1.Describe(context) + " ");
                }
            }

            return sb.ToString();
        }

        /*
        private string OldDescribe(ILanguage language, int severity, int eloquence, int quality)
        {
            var lex = language == null && (severity + eloquence + quality == 0) ? this : Mutate(language, severity, eloquence, quality);

            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(lex.Phrase))
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();
            IEnumerable<ILexica> adjectives = lex.Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive);

            //up-caps all the proper nouns
            if (lex.Type == LexicalType.ProperNoun)
            {
                lex.Phrase = lex.Phrase.ProperCaps();
            }
            else
            {
                lex.Phrase = lex.Phrase.ToLower();
            }

            //Old engine
            switch (lex.Role)
            {
                case GrammaticalType.Descriptive:
                    if (adjectives.Count() > 0)
                    {
                        if (lex.Type == LexicalType.Article || lex.Type == LexicalType.Interjection)
                        {
                            sb.AppendFormat("{0} {1}", lex.Phrase, adjectives.Select(adj => adj.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                        else
                        {
                            sb.AppendFormat("{1} {0}", lex.Phrase, adjectives.Select(adj => adj.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                    }
                    else
                    {
                        sb.Append(lex.Phrase);
                    }
                    break;
                case GrammaticalType.IndirectObject:
                    if ((lex.Type == LexicalType.Noun || lex.Type == LexicalType.ProperNoun) && !lex.Modifiers.Any(mod => mod.Type == LexicalType.Article))
                    {
                        lex.TryModify(LexicalType.Article, GrammaticalType.Descriptive, "a"); //TODO: make an a/an/the thing
                    }

                    sb.Append(AppendDescriptors(adjectives, lex.Phrase, language, severity, eloquence, quality));
                    break;
                case GrammaticalType.DirectObject:
                    if ((lex.Type == LexicalType.Noun || lex.Type == LexicalType.ProperNoun) && !lex.Modifiers.Any(mod => mod.Type == LexicalType.Article))
                    {
                        lex.TryModify(LexicalType.Article, GrammaticalType.Descriptive, "a"); //TODO: make an a/an/the thing
                    }

                    string describedNoun = AppendDescriptors(adjectives, Phrase, language, severity, eloquence, quality);

                    if (lex.Modifiers.Any(mod => mod.Role == GrammaticalType.IndirectObject || mod.Role == GrammaticalType.Descriptive))
                    {
                        string iObj = lex.Modifiers.Where(mod => mod.Role == GrammaticalType.IndirectObject)
                                            .Select(mod => mod.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", iObj, describedNoun);
                    }
                    else
                    {
                        sb.Append(describedNoun);
                    }

                    break;
                case GrammaticalType.Verb:
                    string adverbString = adjectives.Where(adj => adj.Type == LexicalType.Adverb)
                                 .Select(adj => adj.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.AllComma);

                    string adjectiveString = adjectives.Where(adj => adj.Type == LexicalType.Adjective)
                                     .Select(adj => adj.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.OxfordComma);

                    if (lex.Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                    {
                        string dObj = lex.Modifiers.Where(mod => mod.Role == GrammaticalType.DirectObject)
                                            .Select(mod => mod.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.OxfordComma);

                        sb.AppendFormat("{2} {0} {1} {3}", lex.Phrase, dObj, adverbString, adjectiveString);
                    }
                    else
                    {
                        sb.AppendFormat("{1} {0} {2}", lex.Phrase, adverbString, adjectiveString);
                    }

                    break;
                case GrammaticalType.Subject:
                    if ((lex.Type == LexicalType.Noun || lex.Type == LexicalType.ProperNoun) && !lex.Modifiers.Any(mod => mod.Type == LexicalType.Article))
                    {
                        lex.TryModify(LexicalType.Article, GrammaticalType.Descriptive, "the"); //TODO: make an a/an/the thing
                    }

                    string describedSubject = AppendDescriptors(adjectives, Phrase, language, severity, eloquence, quality);

                    if (lex.Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                    {
                        string vObj = lex.Modifiers.Where(mod => mod.Role == GrammaticalType.Verb)
                                            .Select(mod => mod.Describe(language, severity, eloquence, quality)).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", describedSubject, vObj);
                    }
                    else
                    {
                        sb.Append(describedSubject);
                    }

                    break;
            }

            return sb.ToString().Trim();
        }
         
        private string AppendDescriptors(IEnumerable<ILexica> adjectives, string phrase, LexicalContext context)
        {
            string described = phrase;

            if (adjectives.Count() > 0)
            {
                string decorativeString = adjectives.Where(adj => adj.Type != LexicalType.Article && adj.Type != LexicalType.Interjection)
                        .Select(adj => adj.Describe(context))
                        .CommaList(RenderUtility.SplitListType.AllComma);

                ILexica conjunctive = adjectives.FirstOrDefault(adj => adj.Type == LexicalType.Article || adj.Type == LexicalType.Interjection);
                string conjunctiveString = conjunctive != null
                    ? conjunctive.Describe(context) + " "
                    : string.Empty;

                described = string.Format("{1}{2} {0}", phrase, conjunctiveString, decorativeString);
            }

            return described.Trim();
        }
        */

        /// <summary>
        /// Alter the lex entirely including all of its sublex
        /// </summary>
        /// <param name="context">Contextual nature of the request.</param>
        /// <param name="obfuscationLevel">% level of obfuscating this thing (0 to 100).</param>
        /// <returns>the new lex</returns>
        public ILexica Mutate(LexicalContext context, int obfuscationLevel = 0)
        {
            Lexica newLexica = new Lexica(Type, Role, Phrase, context);

            var dict = GetDictata();

            if (Type != LexicalType.ProperNoun && dict != null && (context.Severity + context.Elegance + context.Quality > 0 || context.Language != dict.Language))
            {
                var newDict = Thesaurus.GetSynonym(dict, context);

                newLexica = new Lexica(Type, Role, newDict.Name, context);
            }

            newLexica.TryModify(Modifiers);

            return newLexica;
        }

        private IEnumerable<LexicalSentence> GetSentences(bool omitName)
        {
            var me = new Lexica(Type, Role, Phrase, Context);
            me.TryModify(Modifiers.Where(mod => mod != null));

            List<LexicalSentence> sentences = new List<LexicalSentence>();

            if (omitName)
            {
                var pronounContext = Context;
                pronounContext.Perspective = NarrativePerspective.None;
                pronounContext.Position = LexicalPosition.None;
                pronounContext.Tense = LexicalTense.None;
                pronounContext.Semantics = new HashSet<string>();

                var pronoun = Thesaurus.GetWord(pronounContext, LexicalType.Pronoun);
                me = new Lexica(pronoun.WordType, me.Role, pronoun.Name, me.Context);
                me.TryModify(me.Modifiers);
            }

            List<ILexica> subjects = new List<ILexica>
            {
                me
            };
            subjects.AddRange(me.Modifiers.Where(mod => mod.Role == GrammaticalType.Subject));

            foreach (ILexica subject in subjects)
            {
                var newLex = subject;
                //This is to catch directly described entities, we have to add a verb to it for it to make sense.
                if (subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Descriptive) && !subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                {
                    Lexica newSubject = new Lexica(subject.Type, subject.Role, subject.Phrase, subject.Context);
                    newSubject.TryModify(subject.Modifiers);

                    var verbContext = subject.Context;
                    verbContext.Semantics = new HashSet<string> { "Existential" };
                    var verb = Thesaurus.GetWord(subject.Context, LexicalType.Article);

                    newSubject.TryModify(LexicalType.Article, GrammaticalType.Verb, verb.Name);

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
