using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Context for a lexica
    /// </summary>
    public class LexicalContext
    {
        /// <summary>
        /// The person/thing observing this
        /// </summary>
        public IEntity Observer { get; set; }

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

        public LexicalContext(IEntity observer)
        {
            Tense = LexicalTense.Present;
            Position = LexicalPosition.None;
            Perspective = NarrativePerspective.FirstPerson;
            Determinant = true;
            Possessive = false;
            Plural = false;
            Severity = 0;
            Elegance = 0;
            Quality = 0;
            Observer = observer;

            Semantics = new HashSet<string>();
        }

        public LexicalContext Clone()
        {
            return new LexicalContext(Observer)
            {
                Tense = Tense,
                Position = Position,
                Perspective = Perspective,
                Determinant = Determinant,
                Possessive = Possessive,
                Plural = Plural,
                Severity = Severity,
                Elegance = Elegance,
                Quality = Quality,
                Semantics = Semantics,
                Language = Language,
                GenderForm = GenderForm
            };
        }
    }
}
