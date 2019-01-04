using NetMud.DataStructure.Tile;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Contains the coordinate array of rooms
    /// </summary>
    public interface IMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        ITile[,] CoordinateTilePlane { get; set; }

        /// <summary>
        /// Is this a portion of a larger map (ie not particularly useful for pathing)
        /// </summary>
        bool Partial { get; }
    }
}
