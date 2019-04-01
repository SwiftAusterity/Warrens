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
    }
}
