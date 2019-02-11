using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Context for a lexica
    /// </summary>
    public class LexicalContext
    {
        /// <summary>
        /// Gender of the subject (for pronoun usage)
        /// </summary>
        public IGender GenderForm { get; set; }

        /// <summary>
        /// Is this a specific instance (the) or a generalized instance (a)
        /// </summary>
        public bool Determinant { get; set; }

        /// <summary>
        /// Should the subject be pluralized
        /// </summary>
        public bool Plural { get; set; }

        /// <summary>
        /// How strong is this event (30 being average). Mutates how many adjectives/adverbs get used.
        /// </summary>
        public int Strength { get; set; }
    }
}
