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
        /// Current location this entity is in
        /// </summary>
        IContains CurrentLocation { get; set; }

        /// <summary>
        /// The zone of the current location
        /// </summary>
        /// <returns>The zone</returns>
        IZone GetZone();

        /// <summary>
        /// The locale of the current Location
        /// </summary>
        /// <returns>The locale, might be null</returns>
        ILocale GetLocale();

        /// <summary>
        /// The room of the current location
        /// </summary>
        /// <returns>The room, might be null</returns>
        IRoom GetRoom();
    }
}
