using NetMud.DataStructure.NaturalResource;
using System.Collections.Generic;

namespace NetMud.DataStructure.Zone
{
    /// <summary>
    /// Defines behavior around geological, meterological and biological elements for locations
    /// </summary>
    public interface IEnvironmentData
    {
        /// <summary>
        /// The fudge value for temperature variance
        /// </summary>    
        int TemperatureCoefficient { get; set; }

        /// <summary>
        /// The fudge value for pressure (weather pattern) variance
        /// </summary>
        int PressureCoefficient { get; set; }

        /// <summary>
        /// Natural resources that can spawn here with rate/chance factor
        /// </summary>
        HashSet<INaturalResourceSpawn<IFlora>> FloraResourceSpawn { get; set; }

        /// <summary>
        /// Natural resources that can spawn here with rate/chance factor
        /// </summary>
        HashSet<INaturalResourceSpawn<IFauna>> FaunaResourceSpawn { get; set; }

        /// <summary>
        /// Natural resources that can spawn here with rate/chance factor
        /// </summary>
        HashSet<INaturalResourceSpawn<IMineral>> MineralResourceSpawn { get; set; }

        /// <summary>
        /// The biome this is supposed to be
        /// </summary>
        Biome BaseBiome { get; set; }
    }
}
