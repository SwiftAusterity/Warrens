using NetMud.DataStructure.Architectural.PropertyBinding;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Contains the coordinate array of rooms
    /// </summary>
    public interface IZoneTemplateMap
    {
        /// <summary>
        /// The room plane
        /// </summary>
        [CoordinateTileMapDataBinder]
        long[,] CoordinateTilePlane { get; set; }

        /// <summary>
        /// Collection of NPCs that should spawn on new create/fallback create
        /// </summary>
        [NPCSpawnDataBinder]
        HashSet<NPCSpawn> NPCSpawns { get; set; }

        /// <summary>
        /// Collection of items that should spawn on new create/fallback create
        /// </summary>
        [ItemSpawnDataBinder]
        HashSet<InanimateSpawn> ItemSpawns { get; set; }

        /// <summary>
        /// Get one of the tiles in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ITileTemplate GetTile(Coordinate coordinates);
    }
}
