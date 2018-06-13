namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Type of affect for dispelling purposes
    /// </summary>
    public enum AilmentType
    {
        /// <summary>
        /// Undispelable by magical means, medically treatable
        /// </summary>
        Physical,

        /// <summary>
        /// Is a pathogenic condition
        /// </summary>
        Disease,

        /// <summary>
        /// Is a venom
        /// </summary>
        Poison
    }
}
