using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Rendering;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Collector of rooms, used for weather patterning
    /// </summary>
    public interface IZoneData : ILocationData, IEnvironmentData, IDescribable, IDiscoverableData
    {
        /// <summary>
        /// The midline elevation point "sea level" for this zone
        /// </summary>
        int BaseElevation { get; set; }

        /// <summary>
        /// What world does this belong to
        /// </summary>
        IGaiaData World { get; set; }

        /// <summary>
        /// Templates for generating randomized locales for zones
        /// </summary>
        HashSet<IAdventureTemplate> Templates { get; set; }
    }
}
