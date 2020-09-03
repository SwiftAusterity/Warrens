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
        /// How a word is pronounced
        /// </summary>
        string Phonetics { get; set; }

        /// <summary>
        /// Has this been mapped by the synset already
        /// </summary>
        bool IsSynMapped { get; set; }

        /// <summary>
        /// Has this lexeme been run through the translator for other languages
        /// </summary>
        bool IsTranslated { get; set; }

        /// <summary>
        /// Has this been curated by a human
        /// </summary>
        bool Curated { get; set; }

        /// <summary>
        /// Has this been run through both Mirriam Webster APIs
        /// </summary>
        bool MirriamIndexed { get; set; }

        /// <summary>
        /// Individual meanings and types under this
        /// </summary>
        IDictata[] WordForms { get; set; }

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
        bool MapSynNet();

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

        /// <summary>
        /// Get a word form by criteria
        /// </summary>
        /// <param name="form">the lexical type</param>
        /// <param name="semantics">the semantic meaning</param>
        /// <param name="bestFit">should we grab best fit for meaning or be more exacting</param>
        /// <returns>the word, or nothing</returns>
        IDictata GetForm(LexicalType form, string[] semantics, bool bestFit = true);
    }
}
