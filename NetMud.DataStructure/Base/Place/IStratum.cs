using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// 1 step smaller than World
    /// </summary>
    public interface IStratum : IData
    {
        /// <summary>
        /// Diameter of this stratum in meters
        /// </summary>
        long Diameter { get; set; }

        /// <summary>
        /// What the various layers of the strata are typically composed of
        /// </summary>
        Dictionary<string, IStratumLayer> Layers { get; set; }

        /// <summary>
        /// How hot it is in this stratum generally
        /// </summary>
        Tuple<int, int> AmbientTemperatureRange { get; set; }

        /// <summary>
        /// How humid it is in this stratum generally
        /// </summary>
        Tuple<int, int> AmbientHumidityRange { get; set; }
    }

    /// <summary>
    /// Defines layers of strata
    /// </summary>
    public interface IStratumLayer
    {
        /// <summary>
        /// Material for this layer, can be null for Air layer
        /// </summary>
        IMaterial BaseMaterial { get; set; }

        /// <summary>
        /// Lower Z bound for this layer
        /// </summary>
        long LowerDepth { get; set; }

        /// <summary>
        /// Upper Z bound for this layer
        /// </summary>
        long UpperDepth { get; set; }
    }
}
