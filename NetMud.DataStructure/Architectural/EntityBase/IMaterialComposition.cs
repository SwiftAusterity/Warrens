namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// What materials are made of (alloy)
    /// </summary>
    public interface IMaterialComposition
    {
        /// <summary>
        /// how much of the alloy is this material (1 to 100)
        /// </summary>
        short PercentageOfComposition { get; set; }

        /// <summary>
        /// The material it's made of
        /// </summary>
        IMaterial Material { get; set; }
    }
}
