namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Where the various celestial bodies are along their paths
    /// </summary>
    public interface ICelestialPosition
    {
        /// <summary>
        /// The celestial object
        /// </summary>
        ICelestial CelestialObject { get; set; }

        /// <summary>
        /// Where the celestial object is in its orbit
        /// </summary>
        float Position { get; set; }
    }
}
