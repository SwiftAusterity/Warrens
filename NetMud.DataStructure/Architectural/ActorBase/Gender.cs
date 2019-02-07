namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Framework for entities having a gender
    /// </summary>
    public class Gender
    {
        /// <summary>
        /// Freeform string for entity gender
        /// </summary>      
        public string Name { get; set; }

        /// <summary>
        /// Collective pronoun
        /// </summary>
        public string Collective { get; set; }

        /// <summary>
        /// Possessive pronoun
        /// </summary>
        public string Possessive { get; set; }

        /// <summary>
        /// Basic pronoun
        /// </summary>
        public string Base { get; set; }

        /// <summary>
        /// Adult generalized noun "woman", "man"
        /// </summary>
        public string Adult { get; set; }

        /// <summary>
        /// Child generalized noun "girl", "boy"
        /// </summary>
        public string Child { get; set; }
    }
}
