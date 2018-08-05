namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Contains the coordinate array of rooms
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        long[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Is this a portion of a larger map (ie not particularly useful for pathing)
        /// </summary>
        bool Partial { get; }

        /// <summary>
        /// Gets a single plane back from the 3d matrix
        /// </summary>
        /// <param name="zIndex">the z-index plane to render</param>
        /// <returns>the single z plane rendered to a large ascii string</returns>
        long[,] GetSinglePlane(int zIndex);
    }
}
