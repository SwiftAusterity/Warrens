namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Indicates a data structure has additional descriptives, is part of rendering
    /// </summary>
    public interface IDescribable
    {
        /// <summary>
        /// The description
        /// </summary>
        string Description { get; set; }
    }
}
