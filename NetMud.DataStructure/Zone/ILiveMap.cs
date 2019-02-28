namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Contains the coordinate array of rooms
    /// </summary>
    public interface ILiveMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        string[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Is this a portion of a larger map (ie not particularly useful for pathing)
        /// </summary>
        bool Partial { get; }

        /// <summary>
        /// Gets a single plane back from the 3d matrix
        /// </summary>
        /// <param name="zIndex">the z-index plane to render</param>
        /// <returns>the single z plane rendered to a large ascii string</returns>
        string[,] GetSinglePlane(int zIndex);
    }
}
