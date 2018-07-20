using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// A gramatical element
    /// </summary>
    public interface ILexica : IComparable<ILexica>, IEquatable<ILexica>
    {
        /// <summary>
        /// The type of word this is to the sentence
        /// </summary>
        GrammaticalType Role { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        LexicalType Type { get; set; }

        /// <summary>
        /// The actual word/phrase
        /// </summary>
        string Phrase { get; set; }

        /// <summary>
        /// Modifiers for this lexica. (Modifier, Conjunction)
        /// </summary>
        HashSet<ILexica> Modifiers { get; set; }

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

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="normalization">How much sentence splitting should be done</param>
        /// <param name="verbosity">A measure of how much flourish should be added as well as how far words get synonym-upgraded by "finesse". (0 to 100)</param>
        /// <param name="chronology">The time tensing of the sentence structure</param>
        /// <param name="perspective">The personage of the sentence structure</param>
        /// <param name="omitName">Should we omit the proper name of the initial subject entirely (and only resort to pronouns)</param>
        /// <returns>A long description</returns>
        string Describe(NarrativeNormalization normalization, int verbosity, LexicalTense chronology = LexicalTense.Present,
            NarrativePerspective perspective = NarrativePerspective.SecondPerson, bool omitName = true);

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        IDictata GetDictata();
    }
}
