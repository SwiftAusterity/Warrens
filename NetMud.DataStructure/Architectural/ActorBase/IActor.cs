using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Architectural.ActorBase
{
    /// <summary>
    /// Midpoint interface for entities that have affects and can be declared as Actor for commands and events
    /// </summary>
    public interface IActor : IEntity
    {
        /// <summary>
        /// Returns whether or not this is a player object
        /// </summary>
        /// <returns>if it is a player object</returns>
        bool IsPlayer();
    }
}
