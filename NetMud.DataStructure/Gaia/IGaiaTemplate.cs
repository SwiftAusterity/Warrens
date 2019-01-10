using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Zone;
using System.Collections.Generic;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Backing data for IGaia, configuration settings for each zone-cluster
    /// </summary>
    public interface IGaiaTemplate : ITemplate, IGaiaFramework, ISingleton<IGaia>
    {
        /// <summary>
        /// Celestial bodies for this world
        /// </summary>
        HashSet<ICelestial> CelestialBodies { get; set; }

        /// <summary>
        /// Time keeping for this world
        /// </summary>
        IChronology ChronologicalSystem { get; set; }

        /// <summary>
        /// Get the zones associated with this world
        /// </summary>
        /// <returns>list of zones</returns>
        IEnumerable<IZoneTemplate> GetZones();
    }
}
