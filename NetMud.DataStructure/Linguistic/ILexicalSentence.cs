using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// An individual sentence
    /// </summary>
    public interface ILexicalSentence
    {
        /// <summary>
        /// The type of sentence
        /// </summary>
        SentenceType Type { get; set; }

        /// <summary>
        /// Modifiers of the sentence, prefix or suffix to the sentence is decided by the describe function.
        /// </summary>
        IList<Tuple<ISensoryEvent, short>> Modifiers { get; set; }

        /// <summary>
        /// The first half of the sentence
        /// </summary>
        IList<Tuple<ISensoryEvent, short>> Subject { get; set; }

        /// <summary>
        /// The second half of the sentence
        /// </summary>
        IList<Tuple<ISensoryEvent, short>> Predicate { get; set; }

        /// <summary>
        /// The language this sentence is in
        /// </summary>
        ILanguage Language { get; set; }

        /// <summary>
        /// The type of sense this is
        /// </summary>
        MessagingType SensoryType { get; set; }

        /// <summary>
        /// The mark for punctation based on sentence type
        /// </summary>
        string PunctuationMark { get; }

        /// <summary>
        /// Fluent (returns itself), add a lexica to the sentence, sentence logic applies here
        /// </summary>
        /// <param name="lex"></param>
        /// <returns></returns>
        ILexicalSentence AddEvent(ISensoryEvent lex, bool recursive = true);

        /// <summary>
        /// Write out the sentence
        /// </summary>
        /// <returns>The sentence in full string form.</returns>
        string Describe();

        /// <summary>
        /// Unpack the sentence into an ordered list of lexica to describe out
        /// </summary>
        /// <returns>a pile of lexica</returns>
        IEnumerable<ISensoryEvent> Unpack();

        /// <summary>
        /// Grab the specific event from the sentence that represents the specific role
        /// </summary>
        /// <param name="eventType">the lexical role to grab</param>
        /// <returns>the role event</returns>
        ISensoryEvent GetSpecificEvent(GrammaticalType eventType);
    }
}
