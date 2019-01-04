using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using System;

namespace NetMud.Data.Zones
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
        public ITile[,] CoordinateTilePlane { get; set; }

        /// <summary>
        /// Is this a partial map
        /// </summary>
        public bool Partial { get; private set; }

        public Map(ITile[,] coordinateMap, bool isPartial)
        {
            CoordinateTilePlane = coordinateMap;
            Partial = isPartial;
        }
    }
}
