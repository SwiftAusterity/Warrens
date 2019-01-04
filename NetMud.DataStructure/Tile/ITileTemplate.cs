using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.Tile
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface ITileTemplate : IKeyedData, IVisible, IBelongToAWorld
    {
        /// <summary>
        /// Character->tile interactions
        /// </summary>
        HashSet<IInteraction> Interactions { get; set; }

        /// <summary>
        /// spawn interactions to pass onto entities as they are popped
        /// </summary>
        HashSet<IDecayEvent> DecayEvents { get; set; }

        /// <summary>
        /// Can be walked on
        /// </summary>
        bool Pathable { get; set; }

        /// <summary>
        /// Can be swam in
        /// </summary>
        bool Aquatic { get; set; }

        /// <summary>
        /// Can be flown through?
        /// </summary>
        bool Air { get; set; }

        /// <summary>
        /// The background color for the display
        /// </summary>
        string BackgroundHexColor { get; set; }
    }
}
