using NetMud.DataStructure.Architectural.ActorBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Context for a lexica
    /// </summary>
    public class LexicalContext
    {
        /// <summary>
        /// The language this is derived from
        /// </summary>
        public ILanguage Language { get; set; }

        /// <summary>
        /// Gender of the subject (for pronoun usage)
        /// </summary>
        public IGender GenderForm { get; set; }

        /// <summary>
        /// Chronological tense of word
        /// </summary>
        public LexicalTense Tense { get; set; }

        /// <summary>
        /// Does this indicate some sort of relational positioning
        /// </summary>
        public LexicalPosition Position { get; set; }

        /// <summary>
        /// Personage of the word
        /// </summary>
        public NarrativePerspective Perspective { get; set; }

        /// <summary>
        /// Is this an determinant form or not (usually true)
        /// </summary>
        public bool Determinant { get; set; }

        /// <summary>
        /// Is this a plural form
        /// </summary>
        public bool Plural { get; set; }

        /// <summary>
        /// Is this a possessive form
        /// </summary>
        public bool Possessive { get; set; }

        /// <summary>
        /// Tags that describe the purpose/meaning of the words
        /// </summary>
        public HashSet<string> Semantics { get; set; }

        /// <summary>
        /// Strength rating of word in relation to synonyms
        /// </summary>
        public int Severity { get; set; }

        /// <summary>
        /// Synonym rating for elegance
        /// </summary>
        public int Elegance { get; set; }

        /// <summary>
        /// Finesse synonym rating; execution of form
        /// </summary>
        public int Quality { get; set; }

        public LexicalContext()
        {
            Tense = LexicalTense.Present;
            Position = LexicalPosition.Near;
            Perspective = NarrativePerspective.FirstPerson;
            Determinant = true;
            Possessive = false;
            Plural = false;
            Severity = 0;
            Elegance = 0;
            Quality = 0;
        }
    }
}
