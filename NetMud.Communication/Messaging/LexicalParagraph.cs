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

        public LexicalParagraph(IEnumerable<string> overRide)
        {
            Override = string.Join(Environment.NewLine, overRide);
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
            List<ILexicalSentence> sentences = new();

            foreach (ISensoryEvent sensoryEvent in Events)
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

            StringBuilder sb = new();

            if (Sentences.Count == 0 || overridingContext != null)
            {
                Unpack(overridingContext);
            }

            List<int> removedSentences = new();
            List<ILexicalSentence> finalSentences = new();
            for (int i = 0; i < Sentences.Count(); i++)
            {
                if (removedSentences.Contains(i))
                {
                    continue;
                }

                ILexicalSentence sentence = Sentences[i];
                for (int n = i + 1; n < Sentences.Count(); n++)
                {
                    if (removedSentences.Contains(n))
                    {
                        continue;
                    }

                    ILexicalSentence secondSentence = Sentences[n];
                    foreach (SentenceComplexityRule complexityRule in sentence.Language.ComplexityRules)
                    {
                        short match = complexityRule.MatchesRule(sentence, secondSentence, overridingContext.Elegance);

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

            foreach (ILexicalSentence sentence in finalSentences)
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
                    List<Tuple<ISensoryEvent, short>> newSubjects = new();
                    newSubjects.AddRange(first.Subject);
                    newSubjects.AddRange(second.Subject.Where(sub => !newSubjects.Any(nSub => nSub.Item1.Event.Phrase.Equals(sub.Item1.Event.Phrase, StringComparison.InvariantCultureIgnoreCase))));

                    foreach (ILexica sub in newSubjects.SelectMany(subj => subj.Item1.Event.Modifiers).Where(pre => pre.Role == GrammaticalType.Descriptive))
                    {
                        sub.Context.Plural = true;
                    }

                    foreach (Tuple<ISensoryEvent, short> sub in newSubjects.Where(pre => pre.Item1.Event.Role == GrammaticalType.Verb || pre.Item1.Event.Role == GrammaticalType.ConjugatedVerb))
                    {
                        sub.Item1.Event.Context.Plural = true;
                    }

                    first.Subject = newSubjects;
                    break;
                case -2:
                    List<Tuple<ISensoryEvent, short>> newPredicates = new();
                    newPredicates.AddRange(first.Predicate);
                    newPredicates.AddRange(second.Predicate.Where(sub => !newPredicates.Any(nSub => nSub.Item1.Event.Phrase.Equals(sub.Item1.Event.Phrase, StringComparison.InvariantCultureIgnoreCase))));

                    foreach (ILexica pred in newPredicates.SelectMany(subj => subj.Item1.Event.Modifiers).Where(pre => pre.Role == GrammaticalType.Descriptive))
                    {
                        pred.Context.Plural = true;
                    }

                    foreach (Tuple<ISensoryEvent, short> pred in newPredicates.Where(pre => pre.Item1.Event.Role == GrammaticalType.Verb || pre.Item1.Event.Role == GrammaticalType.ConjugatedVerb))
                    {
                        pred.Item1.Event.Context.Plural = true;
                    }

                    first.Predicate = newPredicates;
                    break;
            }

            List<Tuple<ISensoryEvent, short>> newModifiers = new();
            newModifiers.AddRange(first.Modifiers);
            newModifiers.AddRange(second.Modifiers);

            first.Modifiers = newModifiers;

            return first;
        }
    }
}
