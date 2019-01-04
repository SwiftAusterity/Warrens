using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZoneTemplate : IZoneFramework, IEnvironmentData, ITemplate, ISingleton<IZone>
    {
        /// <summary>
        /// What hemisphere this zone is in
        /// </summary>
        HemispherePlacement Hemisphere { get; set; }

        /// <summary>
        /// Other players who are allowed to modify this zone map
        /// </summary>
        HashSet<IAccount> Cooperative { get; set; }

        /// <summary>
        /// The room plane
        /// </summary>
        IZoneTemplateMap Map { get; set; }

        /// <summary>
        /// What world does this belong to
        /// </summary>
        IGaiaTemplate World { get; set; }

        /// <summary>
        /// Get one of the tiles in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        ITileTemplate GetTile(Coordinate coordinates);

        /// <summary>
        /// Get one of the pathways in the tile map
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        IPathway GetPathway(Coordinate coordinates);
    }
}
