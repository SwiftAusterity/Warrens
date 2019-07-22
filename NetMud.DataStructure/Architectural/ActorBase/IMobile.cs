using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Midpoint entity interface for players/npcs
    /// </summary>
    public interface IMobile : IActor, IContains, IGetTired, ICanBeHarmed, ICanFight
    {
        MobilityState StancePosition { get; set; }
		
        /// <summary>
        /// Held objects for the player
        /// </summary>
        IEntityContainer<IInanimate> Inventory { get; set; }
    }
}
