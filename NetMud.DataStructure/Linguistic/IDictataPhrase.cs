using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IDictataPhrase : IConfigData
    {
        /// <summary>
        /// Things this is the same as mostly
        /// </summary>
        HashSet<IDictata> Words { get; set; }

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
        /// Is this a feminine or masculine word (not related to actual genders but gendered languages)
        /// </summary>
        bool Feminine { get; set; }

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
    }
}
