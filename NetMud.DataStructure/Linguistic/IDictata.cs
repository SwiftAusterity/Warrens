using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IDictata : IWeightedMeaning
    {
        /// <summary>
        /// The unique key language_name_id
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// The text of the word
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The grouping value for synonym tracking
        /// </summary>
        short FormGroup { get; set; }

        /// <summary>
        /// Get the lexeme for this word
        /// </summary>
        /// <returns>the lexeme</returns>
        ILexeme GetLexeme();

        /// <summary>
        /// creates a related dictata and lexeme with a new word
        /// </summary>
        /// <param name="synonym"></param>
        /// <returns></returns>
        ILexeme MakeRelatedWord(ILanguage language, string word, bool synonym, int severity, int elegance, int quality, HashSet<string> semantics
            , IDictata existingWord = null);

        /// <summary>
        /// Create a lexica from this
        /// </summary>
        /// <returns></returns>
        ILexica GetLexica(GrammaticalType role, LexicalContext context);

        /// <summary>
        /// Make a shallow copy of this
        /// </summary>
        /// <returns></returns>
        IDictata Clone();
    }
}
