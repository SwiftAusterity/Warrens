using NetMud.Communication.Lexical;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMud.Data.Linguistic
{
    /// <summary>
    /// An individual sentence
    /// </summary>
    public class LexicalSentence
    {
        /// <summary>
        /// The type of sentence
        /// </summary>
        public SentenceType Type { get; set; }

        /// <summary>
        /// Modifiers of the sentence, prefix or suffix to the sentence is decided by the describe function.
        /// </summary>
        public IList<Tuple<ISensoryEvent, short>> Modifiers { get; set; }

        /// <summary>
        /// The first half of the sentence
        /// </summary>
        public IList<Tuple<ISensoryEvent, short>> Subject { get; set; }

        /// <summary>
        /// The second half of the sentence
        /// </summary>
        public IList<Tuple<ISensoryEvent, short>> Predicate { get; set; }

        /// <summary>
        /// The language this sentence is in
        /// </summary>
        public ILanguage Language { get; set; }

        /// <summary>
        /// The type of sense this is
        /// </summary>
        public MessagingType SensoryType { get; set; }

        /// <summary>
        /// The mark for punctation based on sentence type
        /// </summary>
        public string PunctuationMark
        {
            get
            {
                return LexicalProcessor.GetPunctuationMark(Type);
            }
        }

        public LexicalSentence(ILexica lex)
        {
            Modifiers = new List<Tuple<ISensoryEvent, short>>();
            Subject = new List<Tuple<ISensoryEvent, short>>();
            Predicate = new List<Tuple<ISensoryEvent, short>>();
            Type = SentenceType.Statement;

            SensoryType = MessagingType.Visible;
            SetLanguage(lex.Context?.Language);
            AddEvent(new SensoryEvent(lex, 30, SensoryType));
        }

        public LexicalSentence(ISensoryEvent lex)
        {
            Modifiers = new List<Tuple<ISensoryEvent, short>>();
            Subject = new List<Tuple<ISensoryEvent, short>>();
            Predicate = new List<Tuple<ISensoryEvent, short>>();
            Type = SentenceType.Statement;

            SensoryType = lex.SensoryType;
            SetLanguage(lex.Event.Context?.Language);
            AddEvent(lex);
        }

        private void SetLanguage(ILanguage language)
        {
            if (language == null || language.SentenceRules == null || !language.SentenceRules.Any())
            {
                IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

                Language = globalConfig.BaseLanguage;
            }
            else
            {
                Language = language;
            }
        }

        /// <summary>
        /// Fluent (returns itself), add a lexica to the sentence, sentence logic applies here
        /// </summary>
        /// <param name="lex"></param>
        /// <returns></returns>
        public LexicalSentence AddEvent(ISensoryEvent lex, bool recursive = true)
        {
            //modification rules ordered by specificity
            foreach (var wordRule in Language.Rules.Where(rul => rul.Matches(lex.Event))
                                                               .OrderByDescending(rul => rul.RuleSpecificity()))
            {
                if (wordRule.NeedsArticle && !lex.Event.Modifiers.Any(mod => mod.Type == LexicalType.Article)
                    && (!wordRule.WhenPositional || lex.Event.Context.Position != LexicalPosition.None))
                {
                    var articleContext = lex.Event.Context.Clone();

                    //Make it determinant if the word is plural
                    articleContext.Determinant = lex.Event.Context.Plural || articleContext.Determinant;

                    //If we have position and it's the subject we have to short circuit this
                    if(lex.Event.Role != GrammaticalType.Verb)
                    {
                        articleContext.Position = LexicalPosition.None;
                        articleContext.Perspective = NarrativePerspective.None;
                        articleContext.Tense = LexicalTense.None;
                    }

                    IDictata article = null;
                    if (wordRule.SpecificAddition != null)
                    {
                        article = wordRule.SpecificAddition;
                    }
                    else
                    {
                        article = Thesaurus.GetWord(articleContext, LexicalType.Article);
                    }

                    if (article != null)
                    {
                        lex.TryModify(LexicalType.Article, GrammaticalType.Descriptive, article.Name, false);
                    }
                }

                if (string.IsNullOrWhiteSpace(wordRule.AddPrefix))
                {
                    lex.Event.Phrase = string.Format("{0}{1}", wordRule.AddPrefix, lex.Event.Phrase.Trim());
                }

                if (string.IsNullOrWhiteSpace(wordRule.AddSuffix))
                {
                    lex.Event.Phrase = string.Format("{1}{0}", wordRule.AddSuffix, lex.Event.Phrase.Trim());
                }
            }

            //Positional object modifier
            if(lex.Event.Role == GrammaticalType.DirectObject && lex.Event.Context.Position != LexicalPosition.None && !lex.Event.Modifiers.Any(mod => mod.Role == GrammaticalType.IndirectObject))
            {
                var positionalWord = Thesaurus.GetWord(lex.Event.Context, LexicalType.Article);

                lex.TryModify(new Lexica(LexicalType.Noun, GrammaticalType.IndirectObject, positionalWord.Name, lex.Event.Context));
            }

            var rule = Language.SentenceRules.FirstOrDefault(rul => rul.Type == Type && rul.Fragment == lex.Event.Role);

            if(rule != null)
            {
                //subject
                if(rule.SubjectPredicate)
                {
                    Subject.Add(new Tuple<ISensoryEvent, short>(lex, rule.ModificationOrder));
                }
                else
                {
                    Predicate.Add(new Tuple<ISensoryEvent, short>(lex, rule.ModificationOrder));
                }
            }
            else
            {
                Modifiers.Add(new Tuple<ISensoryEvent, short>(lex, 99));
            }

            if(recursive)
            {
                var newMods = new HashSet<ILexica>();
                foreach (var mod in lex.Event.Modifiers.Where(mod => mod.Role != GrammaticalType.Descriptive && mod.Role != GrammaticalType.None))
                {
                    AddEvent(new SensoryEvent(mod, lex.Strength, lex.SensoryType));
                    newMods.Add(mod);
                }

                lex.Event.Modifiers.RemoveWhere(modi => newMods.Any(mods => mods == modi));
            }

            return this;
        }

        /// <summary>
        /// Unpack the lexica
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        public void Unpack(LexicalContext overridingContext = null)
        {
            var newSubject = new List<Tuple<ISensoryEvent, short>>();
            //Subject
            foreach (var lex in Subject.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.Strength, overridingContext);

                var i = lex.Item2;
                foreach (var subLex in lexes)
                {
                    newSubject.Add(new Tuple<ISensoryEvent, short>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            var newPredicate = new List<Tuple<ISensoryEvent, short>>();
            //Predicate
            foreach (var lex in Predicate.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.Strength, overridingContext);

                var i = lex.Item2;
                foreach (var subLex in lexes)
                {
                    newPredicate.Add(new Tuple<ISensoryEvent, short>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            var newModifiers = new List<Tuple<ISensoryEvent, short>>();
            //Modifiers
            foreach (var lex in Modifiers.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.Strength, overridingContext);

                var i = lex.Item2;
                foreach(var subLex in lexes)
                {
                    newModifiers.Add(new Tuple<ISensoryEvent, short>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            Subject = newSubject;
            Predicate = newPredicate;
            Modifiers = newModifiers;
        }

        /// <summary>
        /// Write out the sentence
        /// </summary>
        /// <returns>The sentence in full string form.</returns>
        public string Describe()
        {
            if(Subject.Count + Predicate.Count <= 1)
            {
                return string.Empty;
            }

            StringBuilder sb = new StringBuilder();

            //Subject
            foreach(var lex in Subject.OrderBy(pair => pair.Item2))
            {
                sb.AppendFormat("{0} ", lex.Item1.Describe());
            }

            //Predicate
            foreach (var lex in Predicate.OrderBy(pair => pair.Item2))
            {
                sb.AppendFormat("{0} ", lex.Item1.Describe());
            }

            //Modifiers
            foreach (var lex in Modifiers.OrderBy(pair => pair.Item2))
            {
                sb.AppendFormat("{0} ", lex.Item1.Describe());
            }

            //Ensure every sentence starts with a caps letter
            //Do punctuation
            return string.Format("{1}{0}{2}",
                sb.ToString().CapsFirstLetter(true).Trim(),
                Language.PrecedentPunctuation ? PunctuationMark : "",
                Language.AntecendentPunctuation ? PunctuationMark : "");
        }
    }
}
