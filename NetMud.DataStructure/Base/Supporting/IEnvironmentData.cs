using NetMud.DataStructure.Behaviors.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Supporting
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
        IDictionary<INaturalResource, int> NaturalResourceSpawn { get; set; }

        /// <summary>
        /// The biome this is supposed to be
        /// </summary>
        Biome BaseBiome { get; set; }
    }
}
