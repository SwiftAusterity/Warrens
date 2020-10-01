using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IWeightedMeaning
    {
        /// <summary>
        /// Human readable definition
        /// </summary>
        MarkdownString Definition { get; set; }

        /// <summary>
        /// The wordform
        /// </summary>
        LexicalType WordType { get; set; }

        /// <summary>
        /// The language this is derived from
        /// </summary>
        ILanguage Language { get; set; }

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
        /// Is this a feminine or masculine word (not related to actual genders but gendered languages)
        /// </summary>
        bool Feminine { get; set; }

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        HashSet<string> Semantics { get; set; }

        /// <summary>
        /// Usage context
        /// </summary>
        SemanticContext Context { get; set; }

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
        /// Synonym rating for offensive
        /// </summary>
        bool Vulgar { get; set; }

        /// <summary>
        /// The number of times this specific wordform has been rated
        /// </summary>
        int TimesRated { get; set; }

        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<IRelatedWord> RelatedWords { get; set; }

        IEnumerable<IDictata> Synonyms { get; }

        IEnumerable<IDictata> Antonyms { get; }
    }
}
