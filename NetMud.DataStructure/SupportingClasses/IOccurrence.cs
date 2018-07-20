using NetMud.DataStructure.Linguistic;

namespace NetMud.DataStructure.SupportingClasses
{
    /// <summary>
    /// Wrapper for pairing perceptive strength with lexica eventing
    /// </summary>
    public interface IOccurrence
    {
        /// <summary>
        /// The thing happening
        /// </summary>
        ILexica Event { get; set; }

        /// <summary>
        /// The perceptive strength (higher = easier to see and greater distance noticed)
        /// </summary>
        int Strength { get; set; }

        /// <summary>
        /// The type of sense used to detect this
        /// </summary>
        MessagingType SensoryType { get; set; }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <param name="conjunction">the joining text</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(ILexica modifier);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <param name="conjunction">the joining text</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(LexicalType type, GrammaticalType role, string phrase);
    }
}
