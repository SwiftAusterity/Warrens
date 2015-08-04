using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Midpoint entity interface for players/npcs
    /// </summary>
    public interface IMobile : IActor, IContains
    {
        /// <summary>
        /// Held objects for the player
        /// </summary>
        IEntityContainer<IInanimate> Inventory { get; set; }
    }
}
