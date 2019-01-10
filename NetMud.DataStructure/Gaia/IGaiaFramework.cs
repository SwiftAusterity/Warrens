namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Backing data for IGaia, configuration settings for each zone-cluster
    /// </summary>
    public interface IGaiaFramework
    {
        /// <summary>
        /// The angle at which this world rotates in space. Irrelevant for fixed objects.
        /// </summary>
        float RotationalAngle { get; set; }
    }
}
