using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZoneFramework : IDescribable
    {
        /// <summary>
        /// What hemisphere this zone is in
        /// </summary>
        HemispherePlacement Hemisphere { get; set; }

        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        int BaseElevation { get; set; }
    }
}
