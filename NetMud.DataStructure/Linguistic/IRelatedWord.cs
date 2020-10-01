using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    public interface IRelatedWord
    {
        /// <summary>
        /// What type of relationship this is
        /// </summary>
        WordRelationalType RelationType { get; set; }
        
        /// <summary>
        /// The related word in question
        /// </summary>
        IDictata Word { get; set; }

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
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        HashSet<string> Semantics { get; set; }
    }
}
