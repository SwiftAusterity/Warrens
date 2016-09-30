using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZone : ILookupData
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        int BaseElevation { get; set; }

        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>
        int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        int PressureCoefficient { get; set; }

        /// <summary>
        /// Who currently owns this zone
        /// </summary>
        long Owner { get; set; } //long for now cause it's supposed to be guild/clan but that wont be implemented for a while, it'll be a bigint in the db anyways

        /// <summary>
        /// Is this zone ownership malleable
        /// </summary>
        bool Claimable { get; set; }
    }
}
