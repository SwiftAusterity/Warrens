using NetMud.DataStructure.Base.EntityBackingData;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Contains the coordinate array of rooms
    /// </summary>
    public interface ILiveMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        IRoom[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Is this a portion of a larger map (ie not particularly useful for pathing)
        /// </summary>
        bool Partial { get; }

        /// <summary>
        /// Renders a single z plane of the map to an ascii string
        /// </summary>
        /// <param name="zIndex">the z-index plane to render</param>
        /// <param name="withPathways">include pathways? (triples the size of the map)</param>
        /// <returns>the single z plane rendered to a large ascii string</returns>
        string RenderToSinglePlane(int zIndex, bool withPathways);
    }

    /// <summary>
    /// Contains the coordinate array of roomData nodes
    /// </summary>
    public interface IBackingDataMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        IRoomData[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Renders a single z plane of the map to an ascii string
        /// </summary>
        /// <param name="zIndex">the z-index plane to render</param>
        /// <param name="forAdmin">should the rooms and pathways render with admin links</param>
        /// <param name="withPathways">include pathways? (triples the size of the map)</param>
        /// <returns>the single z plane rendered to a large ascii string</returns>
        string RenderToSinglePlane(int zIndex, bool forAdmin, bool withPathways);
    }
}
