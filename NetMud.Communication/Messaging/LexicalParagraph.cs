using NetMud.DataStructure.Linguistic;
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

            foreach(var sensoryEvent in Events)
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
                return Override;

            var sb = new StringBuilder();

            if(Sentences.Count == 0 || overridingContext != null)
            {
                Unpack(overridingContext);
            }

            foreach(var sentence in Sentences)
            {
                sb.Append(sentence.Describe() + " ");
            }

            if (sb.Length > 0)
            {
                sb.Length -= 1;
            }

            return sb.ToString();
        }
    }
}
