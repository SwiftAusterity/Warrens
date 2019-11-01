using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IDictata
    {
        /// <summary>
        /// The unique key language_name_id
        /// </summary>
        string UniqueKey { get; }

        /// <summary>
        /// Language for this word
        /// </summary>
        ILanguage Language { get; set; }

        /// <summary>
        /// The text of the word
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The grouping value for synonym tracking
        /// </summary>
        short FormGroup { get; set; }

        /// <summary>
        /// The wordform
        /// </summary>
        LexicalType WordType { get; set; }

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        LexicalTense Tense { get; set; }

        /// <summary>
        /// Does this indicate some sort of relational positioning
        /// </summary>
        LexicalPosition Positional { get; set; }

        /// <summary>
        /// Personage of the word
        /// </summary>
        NarrativePerspective Perspective { get; set; }

        /// <summary>
        /// Is this a feminine or masculine word (not related to actual genders but gendered languages)
        /// </summary>
        bool Feminine { get; set; }

        /// <summary>
        /// Is this an determinant form or not (usually true)
        /// </summary>
        bool Determinant { get; set; }

        /// <summary>
        /// Is this a plural form
        /// </summary>
        bool Plural { get; set; }

        /// <summary>
        /// Is this a possessive form
        /// </summary>
        bool Possessive { get; set; }

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        HashSet<string> Semantics { get; set; }

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        int Severity { get; set; }

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        int Elegance { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        int Quality { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<IDictata> Synonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        HashSet<IDictata> Antonyms { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<IDictataPhrase> PhraseSynonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        HashSet<IDictataPhrase> PhraseAntonyms { get; set; }

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
        ILexeme MakeRelatedWord(ILanguage language, string word, bool synonym);

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
