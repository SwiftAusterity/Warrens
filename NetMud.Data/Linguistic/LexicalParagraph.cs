using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Data.Linguistic
{
    public class LexicalParagraph
    {
        public IList<ISensoryEvent> Events { get; set; }

        internal IList<LexicalSentence> Sentences { get; set; }

        public LexicalParagraph()
        {
            Events = new List<ISensoryEvent>();
            Sentences = new List<LexicalSentence>();
        }

        public LexicalParagraph AddEvent(ISensoryEvent newEvent)
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
            Sentences = new List<LexicalSentence>();

            foreach(var sensoryEvent in Events)
            {
                Sentences.Add(new LexicalSentence(sensoryEvent));
            }
        }

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <returns>A long description</returns>
        public string Describe()
        {
            var sb = new StringBuilder();

            if(Sentences.Count == 0)
            {
                Unpack();
            }

            foreach(var sentence in Sentences)
            {
                sb.Append(sentence.Describe() + " ");
            }

            sb.Length -= 1;

            return sb.ToString();
        }
    }
}
