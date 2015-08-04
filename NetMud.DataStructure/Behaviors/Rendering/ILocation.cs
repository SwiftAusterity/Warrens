using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    /// <summary>
    /// Locations are special containers for entities
    /// </summary>
    public interface ILocation : IContains
    {
        /// <summary>
        /// Get the surrounding locations based on a strength radius
        /// </summary>
        /// <param name="strength">number of places to go out</param>
        /// <returns>list of valid surrounding locations</returns>
        IEnumerable<ILocation> GetSurroundings(int strength);

        /// <summary>
        /// Pathways leading from this room
        /// </summary>
        IEntityContainer<IPathway> Pathways { get; set; }

        /// <summary>
        /// Mobiles (NPC, Players) in the room
        /// </summary>
        IEntityContainer<IMobile> MobilesInside { get; set; }
    }
}
