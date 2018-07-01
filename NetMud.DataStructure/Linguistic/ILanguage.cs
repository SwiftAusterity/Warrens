using NetMud.DataStructure.Base.System;

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
    }
}
