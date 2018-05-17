using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    [Serializable]
    public class Lexica : ILexica
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        public LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        public string Phrase { get; set; }

        /// <summary>
        /// Does this modify another lexica
        /// </summary>
        public ILexica Modifies { get; set; }

        /// <summary>
        /// What phrase do we use to join this to the other lexica
        /// </summary>
        public string Conjunction { get; set; }
    }
}
