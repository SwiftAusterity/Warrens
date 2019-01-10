using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Spoken languages, config data
    /// </summary>
    public interface ILanguage : IConfigData
    {
        /// <summary>
        /// Languages only used for input and output translation
        /// </summary>
        bool UIOnly { get; set; }

        /// <summary>
        /// Google's name for a language for the translation service
        /// </summary>
        string GoogleLanguageCode { get; set; }
    }
}
