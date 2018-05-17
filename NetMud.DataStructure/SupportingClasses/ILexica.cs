namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    public interface ILexica
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        string Phrase { get; set; }

        /// <summary>
        /// Does this modify another lexica
        /// </summary>
        ILexica Modifies { get; set; }

        /// <summary>
        /// What phrase do we use to join this to the other lexica
        /// </summary>
        string Conjunction { get; set; }
    }
}
