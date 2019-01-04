using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Midpoint entity interface for players/npcs
    /// </summary>
    public interface IMobile : IActor, IContains, IGetTired, ICanBeHarmed
    {
        /// <summary>
        /// Held objects for the player
        /// </summary>
        IEntityContainer<IInanimate> Inventory { get; set; }

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        HashSet<IUse> UsableAbilities { get; set; }
    }
}
