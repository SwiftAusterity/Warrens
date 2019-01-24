using NetMud.Communication.Lexical;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Linguistic;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

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

        public Lexica()
        {
            Modifiers = new HashSet<ILexica>();
        }

        public Lexica(LexicalType type, GrammaticalType role, string phrase)
        {
            Type = type;
            Phrase = phrase;
            Role = role;

            Modifiers = new HashSet<ILexica>();

            LexicalProcessor.VerifyDictata(this);
        }

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        public IDictata GetDictata()
        {
            return ConfigDataCache.Get<IDictata>(new ConfigDataCacheKey(typeof(IDictata), string.Format("{0}_{1}", Type.ToString(), Phrase)));
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
                Modifiers.Add(modifier);

            return passthru ? this : modifier;
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(ILexica[] modifier)
        {
            foreach (ILexica mod in modifier)
                TryModify(mod);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public void TryModify(IEnumerable<ILexica> modifier)
        {
            foreach (ILexica mod in modifier)
                TryModify(mod);
        }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        public ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = false)
        {
            Lexica modifier = new Lexica(type, role, phrase);
            if (!Modifiers.Contains(modifier))
                Modifiers.Add(modifier);

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
                TryModify(mod);
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="normalization">How much sentence splitting should be done</param>
        /// <param name="verbosity">A measure of how much flourish should be added as well as how far words get synonym-upgraded by "finesse". (0 to 100)</param>
        /// <param name="chronology">The time tensing of the sentence structure</param>
        /// <param name="perspective">The personage of the sentence structure</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        public string Describe(NarrativeNormalization normalization, int verbosity, LexicalTense chronology = LexicalTense.Present,
            NarrativePerspective perspective = NarrativePerspective.SecondPerson, bool omitName = true)
        {
            IEnumerable<Tuple<SentenceType, ILexica>> sentences = GetSentences(this, normalization, verbosity, chronology, perspective, omitName);

            if (normalization == NarrativeNormalization.Runon)
            {
                return string.Join(" and ", sentences.Select(sentence => sentence.Item2.ToString())).CapsFirstLetter() + LexicalProcessor.GetPunctuationMark(sentences.First().Item1);
            }

            //join the sentences together with a space and add punctuation
            List<string> finalOutput = new List<string>();
            foreach (Tuple<SentenceType, ILexica> sentence in sentences)
            {
                //Ensure every sentence starts with a caps letter
                string sentenceText = sentence.Item2.ToString().CapsFirstLetter(true) + LexicalProcessor.GetPunctuationMark(sentence.Item1);

                finalOutput.Add(sentenceText);
            }

            return string.Join(" ", finalOutput);
        }

        private IEnumerable<Tuple<SentenceType, ILexica>> GetSentences(ILexica me, NarrativeNormalization normalization, 
                                                                        int verbosity, LexicalTense chronology, NarrativePerspective perspective, bool omitName)
        {
            List<Tuple<SentenceType, ILexica>> sentences = new List<Tuple<SentenceType, ILexica>>();

            //TODO: make a get pronoun thing in the thesaurus
            if (omitName)
            {
                me = new Lexica(LexicalType.Pronoun, Role, "it")
                {
                    Modifiers = new HashSet<ILexica>(Modifiers.Where(mod => mod.Role != GrammaticalType.Subject))
                };
            }

            List<ILexica> subjects = new List<ILexica>
            {
                me
            };
            subjects.AddRange(Modifiers.Where(mod => mod.Role == GrammaticalType.Subject));

            bool isMe = true;
            foreach (ILexica subject in subjects)
            {
                List<ILexica> lexicas = new List<ILexica>();
                switch (normalization)
                {
                    case NarrativeNormalization.Hemmingway:
                        if (isMe)
                        {
                            //Don't just add the name in as its own sentence that's cray, doesn't need to be run on present objects as those are handled by the "is here" thing
                            if (subject.Modifiers.Any(mod => mod.Role != GrammaticalType.Descriptive && mod.Role != GrammaticalType.Subject))
                            {
                                lexicas.Add(new Lexica(subject.Type, subject.Role, subject.Phrase)
                                {
                                    Modifiers = new HashSet<ILexica>(subject.Modifiers.Where(mod => mod.Role != GrammaticalType.Descriptive))
                                });
                            }
                        }
                        else
                            lexicas.Add(subject);

                        //We do pronoun replacement after the first instance
                        bool first = isMe;
                        foreach (ILexica adj in subject.Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive))
                        {
                            Lexica newSplitSubject = null;
                            if (omitName || !first)
                                newSplitSubject = new Lexica(LexicalType.Pronoun, subject.Role, "it"); //TODO: make a get pronoun thing in the thesaurus
                            else
                                newSplitSubject = new Lexica(subject.Type, subject.Role, subject.Phrase);

                            newSplitSubject.TryModify(LexicalType.Conjunction, GrammaticalType.Verb, "is").TryModify(adj);
                            lexicas.Add(newSplitSubject);

                            first = false;
                        }
                        break;
                    default:
                        //This is to catch directly described entities
                        if (subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Descriptive) && !subject.Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                        {
                            Lexica newSubject = new Lexica(subject.Type, subject.Role, subject.Phrase);
                            newSubject.TryModify(subject.Modifiers.Where(mod => mod.Type == LexicalType.Conjunction || mod.Role != GrammaticalType.Descriptive));
                            newSubject.TryModify(LexicalType.Conjunction, GrammaticalType.Verb, "is").TryModify(subject.Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive).ToArray());

                            lexicas.Add(newSubject);
                        }
                        else if (subject.Modifiers.Any())
                            lexicas.Add(subject);
                        break;
                }

                foreach (ILexica lex in lexicas)
                {
                    if (lex.Modifiers.Any(mod => mod.Role == GrammaticalType.Subject))
                    {
                        sentences.Add(new Tuple<SentenceType, ILexica>(SentenceType.Partial, lex));

                        //fragment sentences
                        foreach (ILexica subLex in lex.Modifiers.Where(mod => mod.Role == GrammaticalType.Subject))
                            sentences.Add(new Tuple<SentenceType, ILexica>(SentenceType.Statement, subLex));
                    }
                    else
                        sentences.Add(new Tuple<SentenceType, ILexica>(SentenceType.Statement, lex));
                }

                isMe = false;
            }

            return sentences;
        }

        /// <summary>
        /// Render this lexica to a sentence fragment (or whole sentence if it's a Subject role)
        /// </summary>
        /// <returns>a sentence fragment</returns>
        public override string ToString()
        {
            //short circuit empty lexica
            if (string.IsNullOrWhiteSpace(Phrase))
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            IEnumerable<ILexica> adjectives = Modifiers.Where(mod => mod.Role == GrammaticalType.Descriptive);

            //up-caps all the proper nouns
            if (Type == LexicalType.ProperNoun)
                Phrase = Phrase.ProperCaps();
            else
                Phrase = Phrase.ToLower();

            switch (Role)
            {
                case GrammaticalType.Descriptive:
                    if (adjectives.Count() > 0)
                    {
                        if (Type == LexicalType.Conjunction || Type == LexicalType.Interjection)
                        {
                            sb.AppendFormat("{0} {1}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                        else
                        {
                            sb.AppendFormat("{1} {0}", Phrase, adjectives.Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma));
                        }
                    }
                    else
                    {
                        sb.Append(Phrase);
                    }
                    break;
                case GrammaticalType.IndirectObject:
                    if ((Type == LexicalType.Noun || Type == LexicalType.ProperNoun) && !Modifiers.Any(mod => mod.Type == LexicalType.Conjunction))
                        TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "a"); //TODO: make an a/an/the thing

                    sb.Append(AppendDescriptors(adjectives, Phrase));
                    break;
                case GrammaticalType.DirectObject:
                    if ((Type == LexicalType.Noun || Type == LexicalType.ProperNoun) && !Modifiers.Any(mod => mod.Type == LexicalType.Conjunction))
                        TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "a"); //TODO: make an a/an/the thing

                    string describedNoun = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Role == GrammaticalType.IndirectObject || mod.Role == GrammaticalType.Descriptive))
                    {
                        string iObj = Modifiers.Where(mod => mod.Role == GrammaticalType.IndirectObject)
                                            .Select(mod => mod.ToString()).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", iObj, describedNoun);
                    }
                    else
                        sb.Append(describedNoun);

                    break;
                case GrammaticalType.Verb:
                    string adverbString = adjectives.Where(adj => adj.Type == LexicalType.Adverb)
                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                    string adjectiveString = adjectives.Where(adj => adj.Type == LexicalType.Adjective)
                                     .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                    if (Modifiers.Any(mod => mod.Role == GrammaticalType.DirectObject))
                    {
                        string dObj = Modifiers.Where(mod => mod.Role == GrammaticalType.DirectObject)
                                            .Select(mod => mod.ToString()).CommaList(RenderUtility.SplitListType.OxfordComma);

                        sb.AppendFormat("{2} {0} {1} {3}", Phrase, dObj, adverbString, adjectiveString);
                    }
                    else
                        sb.AppendFormat("{1} {0} {2}", Phrase, adverbString, adjectiveString);
                    break;
                case GrammaticalType.Subject:
                    if ((Type == LexicalType.Noun || Type == LexicalType.ProperNoun) && !Modifiers.Any(mod => mod.Type == LexicalType.Conjunction))
                        TryModify(LexicalType.Conjunction, GrammaticalType.Descriptive, "the"); //TODO: make an a/an/the thing

                    string describedSubject = AppendDescriptors(adjectives, Phrase);

                    if (Modifiers.Any(mod => mod.Role == GrammaticalType.Verb))
                    {
                        string vObj = Modifiers.Where(mod => mod.Role == GrammaticalType.Verb)
                                            .Select(mod => mod.ToString()).CommaList(RenderUtility.SplitListType.AllAnd);

                        sb.AppendFormat("{0} {1}", describedSubject, vObj);
                    }
                    else
                        sb.Append(describedSubject);
                    break;
            }

            return sb.ToString().Trim();
        }

        private string AppendDescriptors(IEnumerable<ILexica> adjectives, string phrase)
        {
            string described = phrase;

            if (adjectives.Count() > 0)
            {
                string decorativeString = adjectives.Where(adj => adj.Type != LexicalType.Conjunction && adj.Type != LexicalType.Interjection)
                                                 .Select(adj => adj.ToString()).CommaList(RenderUtility.SplitListType.AllComma);

                ILexica conjunctive = adjectives.FirstOrDefault(adj => adj.Type == LexicalType.Conjunction || adj.Type == LexicalType.Interjection);
                string conjunctiveString = conjunctive != null ? conjunctive.ToString() + " " : string.Empty;

                described = string.Format("{1}{2} {0}", phrase, conjunctiveString, decorativeString);
            }

            return described.Trim();
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
                        return -1;

                    if (other.Phrase.Equals(Phrase) && other.Type == Type)
                        return 1;

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
