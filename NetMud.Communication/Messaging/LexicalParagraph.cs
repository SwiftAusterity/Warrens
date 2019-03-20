using NetMud.DataStructure.Linguistic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NetMud.Communication.Messaging
{
    public class LexicalParagraph : ILexicalParagraph
    {
        /// <summary>
        /// A string override to be used as output
        /// </summary>
        public string Override { get; set; }

        /// <summary>
        /// List of events in the paragraph
        /// </summary>
        public IList<ISensoryEvent> Events { get; private set; }

        /// <summary>
        /// The current list of sentences
        /// </summary>
        internal IList<ILexicalSentence> Sentences { get; set; }

        public LexicalParagraph()
        {
            Events = new List<ISensoryEvent>();
            Sentences = new List<ILexicalSentence>();
        }

        public LexicalParagraph(string overRide)
        {
            Override = overRide;
            Events = new List<ISensoryEvent>();
            Sentences = new List<ILexicalSentence>();
        }

        public LexicalParagraph(ISensoryEvent newEvent)
        {
            Events = new List<ISensoryEvent>() { newEvent };
            Sentences = new List<ILexicalSentence>();
        }

        public LexicalParagraph(IEnumerable<ISensoryEvent> newEvents)
        {
            Events = newEvents.ToList();
            Sentences = new List<ILexicalSentence>();
        }

        /// <summary>
        /// Add an event to the cluster
        /// </summary>
        /// <param name="newEvent">the event to add</param>
        /// <returns>Fluent response</returns>
        public ILexicalParagraph AddEvent(ISensoryEvent newEvent)
        {
            Events.Add(newEvent);

            return this;
        }

        /// <summary>
        /// Unpack the sentences/lexica
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        public void Unpack(LexicalContext overridingContext = null)
        {
            //Clean them out
            var sentences = new List<ILexicalSentence>();

            foreach (var sensoryEvent in Events)
            {
                sentences.AddRange(sensoryEvent.Unpack(overridingContext));
            }

            Sentences = sentences;
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <returns>A long description</returns>
        public string Describe(LexicalContext overridingContext = null)
        {
            if (!string.IsNullOrWhiteSpace(Override))
            {
                return Override;
            }

            var sb = new StringBuilder();

            if (Sentences.Count == 0 || overridingContext != null)
            {
                Unpack(overridingContext);
            }

            var removedSentences = new List<int>();
            var finalSentences = new List<ILexicalSentence>();
            for (var i = 0; i < Sentences.Count(); i++)
            {
                if (removedSentences.Contains(i))
                {
                    continue;
                }

                var sentence = Sentences[i];
                for (var n = i + 1; n < Sentences.Count(); n++)
                {
                    if (removedSentences.Contains(n))
                    {
                        continue;
                    }

                    ILexicalSentence secondSentence = Sentences[n];
                    foreach (var complexityRule in sentence.Language.ComplexityRules)
                    {
                        var match = complexityRule.MatchesRule(sentence, secondSentence, overridingContext.Elegance);

                        if (match != 0)
                        {
                            finalSentences.Add(CombineSentences(sentence, secondSentence, complexityRule, match));
                            removedSentences.Add(i);
                            removedSentences.Add(n);

                            //Short circut the outer for loop
                            n = Sentences.Count();
                            break;
                        }
                    }
                }

                if (!removedSentences.Contains(i))
                {
                    finalSentences.Add(sentence);
                }
            }

            foreach (var sentence in finalSentences)
            {
                sb.Append(sentence.Describe() + " ");
            }

            if (sb.Length > 0)
            {
                sb.Length -= 1;
            }

            return sb.ToString();
        }

        private ILexicalSentence CombineSentences(ILexicalSentence first, ILexicalSentence second, SentenceComplexityRule rule, int matchBasis)
        {
            /// -1 = subject/subject match
            /// -2 = predicate/predicate match
            /// 0 = no match
            /// 1 = subject/predicate match
            /// 2 = predicate/subject match</returns>

            switch (matchBasis)
            {
                case 1:
                    first.Predicate = second.Predicate;
                    break;
                case 2:
                    first.Subject = second.Subject;
                    break;
                case -1:
                    var newSubjects = new List<Tuple<ISensoryEvent, short>>();
                    newSubjects.AddRange(first.Subject);
                    newSubjects.AddRange(second.Subject);

                    foreach(var sub in newSubjects.Where(pre => pre.Item1.Event.Role == GrammaticalType.Descriptive))
                    {
                        sub.Item1.Event.Context.Plural = true;
                    }

                    first.Subject = newSubjects;
                    break;
                case -2:
                    var newPredicates = new List<Tuple<ISensoryEvent, short>>();
                    newPredicates.AddRange(first.Predicate);
                    newPredicates.AddRange(second.Predicate);

                    foreach (var pred in newPredicates.Where(pre => pre.Item1.Event.Role == GrammaticalType.Descriptive))
                    {
                        pred.Item1.Event.Context.Plural = true;
                    }

                    first.Predicate = newPredicates;
                    break;
            }

            var newModifiers = new List<Tuple<ISensoryEvent, short>>();
            newModifiers.AddRange(first.Modifiers);
            newModifiers.AddRange(second.Modifiers);

            first.Modifiers = newModifiers;

            return first;
        }
    }
}
