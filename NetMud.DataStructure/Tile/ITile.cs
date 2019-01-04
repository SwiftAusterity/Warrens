using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.DataStructure.Tile
{
    /// <summary>
    /// Definition of a live tile
    /// </summary>
    public interface ITile : IHaveQualities, IHaveInfo
    {
        /// <summary>
        /// Where this tile is in the zone map
        /// </summary>
        Coordinate Coordinate { get; set; }

        /// <summary>
        /// What kind of tile this is
        /// </summary>
        ITileTemplate Type { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// The locale this belongs to
        /// </summary>
        IZone ParentLocation { get; set; }

        /// <summary>
        /// Is something in this tile? Only one thing at a time (except for items which can stack)
        /// </summary>
        IEntity TopContents();

        /// <summary>
        /// Get the full stack of contents, only applicable with inanimates
        /// </summary>
        IEnumerable<IInanimate> StackedContents();

        /// <summary>
        /// Does this tile have a pathway?
        /// </summary>
        /// <returns>the pathway</returns>
        IPathway Pathway();

        /// <summary>
        /// Get the visibile celestials. Depends on luminosity, viewer perception and celestial positioning
        /// </summary>
        /// <param name="viewer">Whom is looking</param>
        /// <returns>What celestials are visible</returns>
        IEnumerable<ICelestial> GetVisibileCelestials(IEntity viewer);

        /// <summary>
        /// Get the current luminosity rating of the place you're in
        /// </summary>
        /// <returns>The current Luminosity</returns>
        float GetCurrentLuminosity();

        /// <summary>
        /// Renders out an ascii map of this room plus all rooms in the radius
        /// </summary>
        /// <param name="radius">how far away to render</param>
        /// <returns>the string</returns>
        string RenderCenteredMap(short radiusX, short radiusY, bool visibleOnly, IActor protagonist, bool adminOnly);

        /// <summary>
        /// Run the decay events of this tile
        /// </summary>
        void ProcessDecayEvents();
    }
}
