namespace NetMud.DataStructure.Linguistic
{
    /// <summary>
    /// Rules that identify when words convert to other words based on placement
    /// </summary>
    public interface IDictataTransformationRule
    {
        /// <summary>
        /// The word to be transformed
        /// </summary>
        IDictata Origin { get; set; }

        /// <summary>
        /// A specific word that follows the first
        /// </summary>
        IDictata SpecificFollowing { get; set; }

        /// <summary>
        /// Only when the following word ends with this string
        /// </summary>
        string EndsWith { get; set; }

        /// <summary>
        /// Only when the following word begins with this string
        /// </summary>
        string BeginsWith { get; set; }

        /// <summary>
        /// The word this turns into
        /// </summary>
        IDictata TransformedWord { get; set; }
    }
}
