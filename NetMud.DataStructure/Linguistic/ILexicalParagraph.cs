using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface ILexicalParagraph
    {
        /// <summary>
        /// A string override to be used as output
        /// </summary>
        string Override { get; set; }

        /// <summary>
        /// List of events in the paragraph
        /// </summary>
        IList<ISensoryEvent> Events { get; }

        /// <summary>
        /// Add an event to the cluster
        /// </summary>
        /// <param name="newEvent">the event to add</param>
        /// <returns>Fluent response</returns>
        ILexicalParagraph AddEvent(ISensoryEvent newEvent);

        /// <summary>
        /// Unpack the sentences/lexica
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        void Unpack(LexicalContext overridingContext = null);

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <returns>A long description</returns>
        string Describe(LexicalContext overridingContext = null);
    }
}
