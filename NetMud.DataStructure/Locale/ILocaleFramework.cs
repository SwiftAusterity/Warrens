using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Zone;

namespace NetMud.DataStructure.Locale
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface ILocaleFramework : IDiscoverableData, IDescribable, ISingleton<ILocale>
    {
        /// <summary>
        /// The map of the rooms inside
        /// </summary>
        IMap Interior { get; set; }

        /// <summary>
        /// Get the basic map render for the zone
        /// </summary>
        /// <returns>the zone map in ascii</returns>
        string RenderMap(int zIndex, bool forAdmin = false);

        /// <summary>
        /// The diameter of the zone
        /// </summary>
        /// <returns>the diameter of the zone in room count</returns>
        Dimensions Diameter();

        /// <summary>
        /// Calculate the theoretical dimensions of the zone in inches
        /// </summary>
        /// <returns>the dimensions of the zone in inches</returns>
        Dimensions FullDimensions();
    }
}
