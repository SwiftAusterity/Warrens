using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZone : IReferenceData
    {
        int BaseElevation { get; set; }

        int TemperatureCoefficient { get; set; }

        int PressureCoefficient { get; set; }

        long Owner { get; set; } //long for now cause it's supposed to be guild/clan but that wont be implemented for a while, it'll be a bigint in the db anyways

        bool Claimable { get; set; }
    }
}
