namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Rules that identify contractions
    /// </summary>
    public interface IContractionRule
    {
        /// <summary>
        /// One of the words in the contraction (not an indicator of order)
        /// </summary>
        IDictata First { get; set; }

        /// <summary>
        /// One of the words in the contraction (not an indicator of order)
        /// </summary>
        IDictata Second { get; set; }

        /// <summary>
        /// The contraction this turns into
        /// </summary>
        IDictata Contraction { get; set; }
    }
}
