using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Wrapper for pairing perceptive strength with lexica eventing
    /// </summary>
    public interface ISensoryEvent
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
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(ILexica modifier, bool passthru = false);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(ILexica[] modifier);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(IEnumerable<ILexica> modifier);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(ISensoryEvent modifier, bool passthru = false);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(ISensoryEvent[] modifier);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(IEnumerable<ISensoryEvent> modifier);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = false);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = false);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier);

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="overridingContext">Context to override the lexica with</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        string Unpack(LexicalContext overridingContext = null, bool omitName = true);

        /// <summary>
        /// Describe the lexica
        /// </summary>
        /// <returns></returns>
        string Describe();
    }
}
