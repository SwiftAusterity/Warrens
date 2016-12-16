using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// Defines behavior around geological, meterological and biological elements for locations
    /// </summary>
    public interface IEnvironment
    {
        /// <summary>
        /// Base humidity for this
        /// </summary>
        int Humidity { get; set; }

        /// <summary>
        /// Base temperature for this
        /// </summary>
        int Temperature { get; set; }

        /// <summary>
        /// Natural resources spawned to this location currently
        /// </summary>
        Dictionary<Tuple<long, long, long>, Tuple<INaturalResource, int>> NaturalResources { get; set; }

        /// <summary>
        /// Current humidity for this
        /// </summary>
        /// <returns>1-100 range of humidity</returns>
        int EffectiveHumidity();

        /// <summary>
        /// Current temperature for this
        /// </summary>
        /// <returns>The current temperature in in-game units</returns>
        int EffectiveCurrentTemperature();

        /// <summary>
        /// Is this considered outdoors (ie clear path to the sky)
        /// </summary>
        /// <returns>is it outside or not</returns>
        bool IsOutside();

        /// <summary>
        /// What biome would this be considered currently
        /// </summary>
        /// <returns>the biome</returns>
        Biome GetBiome();
    }

    /// <summary>
    /// What effective environment something is
    /// </summary>
    public enum Biome
    {
        Air,
        Aquatic,
        AquaticSurface,
        AquaticFloor,
        Cavernous,
        Desert,
        Fabricated,
        Forest,
        Mountainous,
        Plains,
        Rainforest,
        Swamp
    }
}
