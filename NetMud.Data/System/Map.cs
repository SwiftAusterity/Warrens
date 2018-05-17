using NetMud.DataStructure.Base.Place;
using System;

namespace NetMud.Data.System
{
    /// <summary>
    /// A 3d matrix map of rooms
    /// </summary>
    [Serializable]
    public class Map : IMap
    {
        /// <summary>
        /// The map of room IDs
        /// </summary>
        public long[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Is this a partial map
        /// </summary>
        public bool Partial { get; private set; }

        public Map(long[,,] coordinateMap, bool isPartial)
        {
            CoordinatePlane = coordinateMap;
            Partial = isPartial;
        }

        /// <summary>
        /// Get a single flat plane of the main map at a specific zIndex
        /// </summary>
        /// <param name="zIndex">the Z (up/down) level to retrieve</param>
        /// <returns></returns>
        public long[,] GetSinglePlane(int zIndex)
        {
            return Cartography.Cartographer.GetSinglePlane(CoordinatePlane, zIndex);
        }
    }
}
