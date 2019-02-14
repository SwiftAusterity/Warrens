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
                foreach(var mod in lex.Modifiers.Where(mod => mod.Role != GrammaticalType.Descriptive &&  mod.Role != GrammaticalType.None))
                {
                    AddLexica(mod);
                }
            }

            return this;
        }

        /// <summary>
        /// Write out the sentence
        /// </summary>
        /// <returns>The sentence in full string form.</returns>
        public string Describe()
        {
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
            string sentenceText = string.Format("{1}{0}{2}",
                sb.ToString().CapsFirstLetter(true).Trim(),
                Language.PrecedentPunctuation ? PunctuationMark : "",
                Language.AntecendentPunctuation ? PunctuationMark : "");

            return sb.ToString();
        }
    }
}
