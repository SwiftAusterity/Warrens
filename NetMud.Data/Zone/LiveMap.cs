using NetMud.DataStructure.Zone;
using System;

namespace NetMud.Data.Zone
{
    /// <summary>
    /// A 3d matrix map of rooms
    /// </summary>
    [Serializable]
    public class LiveMap : ILiveMap
    {
        /// <summary>
        /// The map of room IDs
        /// </summary>
        public string[,,] CoordinatePlane { get; set; }

        /// <summary>
        /// Is this a partial map
        /// </summary>
        public bool Partial { get; private set; }

        public LiveMap(string[,,] coordinateMap, bool isPartial)
        {
            CoordinatePlane = coordinateMap;
            Partial = isPartial;
        }

        /// <summary>
        /// Get a single flat plane of the main map at a specific zIndex
        /// </summary>
        /// <param name="zIndex">the Z (up/down) level to retrieve</param>
        /// <returns></returns>
        public string[,] GetSinglePlane(int zIndex)
        {
            return Cartography.Cartographer.GetSinglePlane(CoordinatePlane, zIndex);
        }
    }
}
