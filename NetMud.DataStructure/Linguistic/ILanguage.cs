using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Spoken languages, config data
    /// </summary>
    public interface ILanguage : IConfigData
    {
        /// <summary>
        /// Google's name From a language From the translation service
        /// </summary>
        string GoogleLanguageCode { get; set; }

        /// <summary>
        /// Languages only used From input and output translation, RW languages
        /// </summary>
        bool UIOnly { get; set; }

        /// <summary>
        /// Does this language have gendered grammar (like most latin based)
        /// </summary>
        bool Gendered { get; set; }

        /// <summary>
        /// Does punctuation come at the beginning of a sentence? (spanish)
        /// </summary>
        bool PrecedentPunctuation { get; set; }

        /// <summary>
        /// Does punctuation come at the end of a sentence?
        /// </summary>
        bool AntecendentPunctuation { get; set; }

        /// <summary>
        /// Rules for sentence construction
        /// </summary>
        HashSet<SentenceGrammarRule> SentenceRules { get; set; }

        /// <summary>
        /// List of grammatical rules to use in sentence construction
        /// </summary>
        HashSet<IGrammarRule> Rules { get; set; }

        /// <summary>
        /// The base needed words for a language to function
        /// </summary>
        BaseLanguageMembers BaseWords { get; set; }
    }
}
