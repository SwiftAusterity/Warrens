using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface ILexeme : IConfigData
    {
        /// <summary>
        /// The language this is derived from
        /// </summary>
        ILanguage Language { get; set; }

        /// <summary>
        /// Has this been mapped by the synset already
        /// </summary>
        bool IsSynMapped { get; set; }

        /// <summary>
        /// Individual meanings and types under this
        /// </summary>
        HashSet<IDictata> WordForms { get; set; }

        /// <summary>
        /// What types exist within the valid wordforms
        /// </summary>
        /// <returns></returns>
        IEnumerable<LexicalType> ContainedTypes();

        /// <summary>
        /// Add language translations for this
        /// </summary>
        void FillLanguages();

        /// <summary>
        /// Map the synnet of this word
        /// </summary>
        void MapSynNet(bool cascade = false);

        /// <summary>
        /// Add a new word form to this lexeme
        /// </summary>
        /// <param name="newWord">The word</param>
        /// <returns>the word with changes</returns>
        IDictata AddNewForm(IDictata newWord);

        /// <summary>
        /// Get a wordform by grouping id
        /// </summary>
        /// <param name="formGroup">the form grouping id</param>
        /// <returns>the word</returns>
        IDictata GetForm(short formGroup);

        /// <summary>
        /// Get a wordform by grouping id
        /// </summary>
        /// <param name="wordType">the lexical type of the word</param>
        /// <param name="formGroup">the form grouping id</param>
        /// <returns>the word</returns>
        IDictata GetForm(LexicalType wordType, short formGroup = -1);
    }
}
