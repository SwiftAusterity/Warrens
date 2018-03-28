using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition
    {
        /// <summary>
        /// The zone this is in
        /// </summary>
        IZone CurrentZone { get; set; }

        /// <summary>
        /// Current location this entity is in
        /// </summary>
        ILocation CurrentLocation { get; set; }
    }
}
