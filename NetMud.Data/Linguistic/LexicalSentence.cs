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
        public IList<Tuple<ILexica, short>> Modifiers { get; set; }

        /// <summary>
        /// The first half of the sentence
        /// </summary>
        public IList<Tuple<ILexica, short>> Subject { get; set; }

        /// <summary>
        /// The second half of the sentence
        /// </summary>
        public IList<Tuple<ILexica, short>> Predicate { get; set; }

        /// <summary>
        /// The language this sentence is in
        /// </summary>
        public ILanguage Language { get; set; }

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
            Modifiers = new List<Tuple<ILexica, short>>();
            Subject = new List<Tuple<ILexica, short>>();
            Predicate = new List<Tuple<ILexica, short>>();
            Type = SentenceType.Statement;
            SetLanguage(lex.Context?.Language);
            AddLexica(lex);
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
        public LexicalSentence AddLexica(ILexica lex, bool recursive = true)
        {
            //modification rules ordered by specificity
            foreach (var wordRule in Language.Rules.Where(rul => rul.Matches(lex))
                                                               .OrderByDescending(rul => rul.RuleSpecificity()))
            {
                if (wordRule.NeedsArticle && !lex.Modifiers.Any(mod => mod.Type == LexicalType.Article))
                {
                    var articleContext = lex.Context;
                    articleContext.Determinant = !lex.Context.Plural;

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
                    lex.Phrase = string.Format("{0}{1}", wordRule.AddPrefix, lex.Phrase.Trim());
                }

                if (string.IsNullOrWhiteSpace(wordRule.AddSuffix))
                {
                    lex.Phrase = string.Format("{1}{0}", wordRule.AddSuffix, lex.Phrase.Trim());
                }
            }

            var rule = Language.SentenceRules.FirstOrDefault(rul => rul.Type == Type && rul.Fragment == lex.Role);

            if(rule != null)
            {
                //subject
                if(rule.SubjectPredicate)
                {
                    Subject.Add(new Tuple<ILexica, short>(lex, rule.ModificationOrder));
                }
                else
                {
                    Predicate.Add(new Tuple<ILexica, short>(lex, rule.ModificationOrder));
                }
            }
            else
            {
                Modifiers.Add(new Tuple<ILexica, short>(lex, 99));
            }

            if(recursive)
            {
                var newMods = new HashSet<ILexica>();
                foreach (var mod in lex.Modifiers.Where(mod => mod.Role != GrammaticalType.Descriptive &&  mod.Role != GrammaticalType.None))
                {
                    AddLexica(mod);
                    newMods.Add(mod);
                }

                lex.Modifiers.RemoveWhere(modi => newMods.Any(mods => mods == modi));
            }

            return this;
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
