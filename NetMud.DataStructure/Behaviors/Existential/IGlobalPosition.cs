using NetMud.DataStructure.Behaviors.Rendering;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition
    {
        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IContains CurrentLocation { get; set; }
    }
}
