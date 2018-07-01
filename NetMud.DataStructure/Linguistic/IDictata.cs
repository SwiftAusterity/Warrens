using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IDictata : IConfigData
    {
        /// <summary>
        /// The language this is derived from
        /// </summary>
        ILanguage Language { get; set; }

        /// <summary>
        /// The type of word this is in general
        /// </summary>
        LexicalType WordType { get; set; }

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        LexicalTense Tense { get; set; }

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
        IEnumerable<IDictata> Synonyms { get; set; }

        /// <summary>
        /// Things this is specifically opposite of mostly
        /// </summary>
        IEnumerable<IDictata> Antonyms { get; set; }
    }
}
