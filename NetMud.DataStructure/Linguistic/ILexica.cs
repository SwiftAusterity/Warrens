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
        /// The context for this, which gets passed downards to anything modifying it
        /// </summary>
        LexicalContext Context { get; set; }

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(ILexica modifier, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(ILexica[] modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(IEnumerable<ILexica> modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(HashSet<ILexica> modifiers);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(LexicalType type, GrammaticalType role, string phrase, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        ILexica TryModify(Tuple<LexicalType, GrammaticalType, string> modifier, bool passthru = true);

        /// <summary>
        /// Try to add a modifier to a lexica
        /// </summary>
        /// <param name="modifier">the lexica that is the modifier</param>
        /// <returns>Whether or not it succeeded</returns>
        void TryModify(Tuple<LexicalType, GrammaticalType, string>[] modifier);

        /// <summary>
        /// Create a narrative description from this
        /// </summary>
        /// <param name="overridingContext">The full lexical context</param>
        /// <returns>A long description</returns>
        IEnumerable<ILexica> Unpack(int strength, LexicalContext overridingContext = null);

        /// <summary>
        /// Describe the lexica
        /// </summary>
        /// <param name="context">The full lexical context</param>
        /// <returns></returns>
        string Describe();

        /// <summary>
        /// Alter the lex entirely including all of its sublex
        /// </summary>
        /// <param name="context">The full lexical context</param>
        /// <param name="obfuscationLevel">how much we should obscure the actual description</param>
        /// <returns>the new lex</returns>
        ILexica Mutate(int obfuscationLevel = 0);

        /// <summary>
        /// Get the dictata from this lexica
        /// </summary>
        /// <returns>A dictata</returns>
        IDictata GetDictata();

        /// <summary>
        /// Generates a new dictata
        /// </summary>
        /// <returns>the dictata</returns>
        IDictata GenerateDictata();
    }
}
