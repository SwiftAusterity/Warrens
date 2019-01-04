using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Tile;
using NetMud.DataStructure.Zone;
using NetMud.Utility;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Zones
{
    /// <summary>
    /// A 3d matrix map of rooms
    /// </summary>
    [Serializable]
    public class ZoneTemplateMap : IZoneTemplateMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        public long[,] CoordinateTilePlane { get; set; }

        /// <summary>
        /// Collection of NPCs that should spawn on new create/fallback create
        /// </summary>
        public HashSet<NPCSpawn> NPCSpawns { get; set; }

        /// <summary>
        /// Collection of items that should spawn on new create/fallback create
        /// </summary>
        public HashSet<InanimateSpawn> ItemSpawns { get; set; }

        public ZoneTemplateMap()
        {
            CoordinateTilePlane = new long[100, 100];
            CoordinateTilePlane.Populate(-1);
            NPCSpawns = new HashSet<NPCSpawn>();
            ItemSpawns = new HashSet<InanimateSpawn>();
        }

        public ZoneTemplateMap(long[,] coordinateMap)
        {
            CoordinateTilePlane = coordinateMap;
        }

        public ZoneTemplateMap(long[,] coordinateMap, HashSet<NPCSpawn> npcSpawns, HashSet<InanimateSpawn> itemSpawns)
        {
            CoordinateTilePlane = coordinateMap;
            NPCSpawns = npcSpawns;
            ItemSpawns = itemSpawns;
        }

        /// <summary>
        /// Get one of the tiles in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public ITileTemplate GetTile(Coordinate coordinates)
        {
            return TemplateCache.Get<ITileTemplate>(CoordinateTilePlane[coordinates.X, coordinates.Y]);
        }
    }
}
