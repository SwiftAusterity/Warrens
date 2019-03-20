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
    public class LexicalSentence : ILexicalSentence
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
        public ILexicalSentence AddEvent(ISensoryEvent lex, bool recursive = true)
        {
            lex.Event.Context.Language = Language;

            //Contractive rules
            var lexDict = lex.Event.GetDictata();
            foreach (var contractionRule in Language.ContractionRules.Where(rul => rul.First == lexDict || rul.Second == lexDict))
            {
                if (!lex.Event.Modifiers.Any(mod => contractionRule.First.Equals(mod.GetDictata()) || contractionRule.Second.Equals(mod.GetDictata())))
                {
                    continue;
                }

                lex.Event.Modifiers.RemoveWhere(mod => contractionRule.First.Equals(mod.GetDictata()) || contractionRule.Second.Equals(mod.GetDictata()));

                lex.Event.Phrase = contractionRule.Contraction.Name;
            }

            //Sentence placement rules
            var rule = Language.SentenceRules.FirstOrDefault(rul => rul.Type == Type && rul.Fragment == lex.Event.Role);

            if (rule != null)
            {
                //subject
                if (rule.SubjectPredicate)
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

            if (recursive)
            {
                var newMods = new HashSet<ILexica>();
                foreach (var mod in lex.Event.Modifiers.Where(mod => mod.Role != GrammaticalType.None && mod.Role != GrammaticalType.Descriptive))
                {
                    AddEvent(new SensoryEvent(mod, lex.Strength, lex.SensoryType));
                    newMods.Add(mod);
                }

                lex.Event.Modifiers.RemoveWhere(modi => newMods.Any(mods => mods.Equals(modi)));
            }

            return this;
        }

        /// <summary>
        /// Write out the sentence
        /// </summary>
        /// <returns>The sentence in full string form.</returns>
        public string Describe()
        {
            if (Subject.Count + Predicate.Count <= 1)
            {
                return string.Empty;
            }

            var lexes = Unpack();
            StringBuilder sb = new StringBuilder();

            //Listable pass rules
            var lexList = new List<Tuple<ISensoryEvent[], int>>();
            var i = 0;
            foreach (var lexPair in lexes.GroupBy(lexi => new { lexi.Event.Role, lexi.Event.Type, lexi.Event.Context.Position }))
            {
                var rule = Language.WordRules.OrderByDescending(rul => rul.RuleSpecificity())
                                                 .FirstOrDefault(rul => rul.Matches(lexPair.First().Event));

                if (rule != null && rule.Listable)
                {
                    lexList.Add(new Tuple<ISensoryEvent[], int>(lexPair.ToArray(), rule.ModificationOrder + i++));
                }
                else
                {
                    foreach (var modifier in lexPair)
                    {
                        lexList.Add(new Tuple<ISensoryEvent[], int>(new ISensoryEvent[] { modifier }, i++));
                    }
                }
            }

            foreach (var grouping in lexList.OrderBy(mod => mod.Item2))
            {
                if (grouping.Item1.Length > 1)
                {
                    sb.Append(grouping.Item1.Select(lexi => lexi.Describe()).CommaList(RenderUtility.SplitListType.OxfordComma) + " ");
                }
                else
                {
                    ISensoryEvent item = grouping.Item1[0];

                    sb.Append(item.Describe() + " ");
                }
            }

            //Ensure every sentence starts with a caps letter
            //Do punctuation
            return string.Format("{1}{0}{2}",
                sb.ToString().CapsFirstLetter(true).Trim(),
                Language.PrecedentPunctuation ? PunctuationMark : "",
                Language.AntecendentPunctuation ? PunctuationMark : "");
        }

        /// <summary>
        /// Unpack the lexica
        /// </summary>
        public IEnumerable<ISensoryEvent> Unpack()
        {
            var wordList = new List<Tuple<ISensoryEvent, int>>();

            //Subject
            foreach (var lex in Subject.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.SensoryType, lex.Item1.Strength);

                int i = lex.Item2 + -100000;
                foreach (var subLex in lexes)
                {
                    wordList.Add(new Tuple<ISensoryEvent, int>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            //Predicate
            foreach (var lex in Predicate.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.SensoryType, lex.Item1.Strength);

                var i = lex.Item2;
                foreach (var subLex in lexes)
                {
                    wordList.Add(new Tuple<ISensoryEvent, int>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            //Modifiers
            foreach (var lex in Modifiers.OrderBy(pair => pair.Item2))
            {
                var lexes = lex.Item1.Event.Unpack(lex.Item1.SensoryType, lex.Item1.Strength);

                var i = lex.Item2 + 100000;
                foreach (var subLex in lexes)
                {
                    wordList.Add(new Tuple<ISensoryEvent, int>(new SensoryEvent(subLex, lex.Item1.Strength, lex.Item1.SensoryType), i++));
                }
            }

            //Transformational word pair rules
            foreach (var rule in Language.TransformationRules.Where(rul => rul.TransformedWord != null && wordList.Any(pair => rul.Origin.Equals(pair.Item1.Event.GetDictata()))))
            {
                var beginsWith = rule.BeginsWith.Split('|', StringSplitOptions.RemoveEmptyEntries);
                var endsWith = rule.EndsWith.Split('|', StringSplitOptions.RemoveEmptyEntries);

                foreach (var lexPair in wordList.Where(pair => rule.Origin.Equals(pair.Item1.Event.GetDictata())))
                {
                    var lex = lexPair.Item1.Event;
                    var nextEvent = wordList.OrderBy(word => word.Item2).FirstOrDefault(word => word.Item2 > lexPair.Item2);

                    if (nextEvent == null)
                    {
                        continue;
                    }

                    var nextLex = nextEvent.Item1.Event;
                    if ((rule.SpecificFollowing != null && nextLex.GetDictata().Equals(rule.SpecificFollowing))
                        || (beginsWith.Count() == 0 || beginsWith.Any(bw => nextLex.Phrase.StartsWith(bw)))
                        || (endsWith.Count() == 0 || endsWith.Any(ew => nextLex.Phrase.EndsWith(ew))))
                    {
                        lex.Phrase = rule.TransformedWord.Name;
                    }
                }
            }

            return wordList.OrderBy(words => words.Item2).Select(words => words.Item1);
        }

        /// <summary>
        /// Grab the specific event from the sentence that represents the specific role
        /// </summary>
        /// <param name="eventType">the lexical role to grab</param>
        /// <returns>the role event</returns>
        public ISensoryEvent GetSpecificEvent(GrammaticalType eventType)
        {
            var events = Unpack();

            return events.FirstOrDefault(ev => ev.Event.Role == eventType);
        }
    }
}
