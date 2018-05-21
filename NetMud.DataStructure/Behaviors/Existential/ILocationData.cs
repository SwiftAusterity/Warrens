using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    public interface ILocationData : IEntityBackingData, ISingleton<ILocation>
    {
        /// <summary>
        /// What pathways are affiliated with this (what it spawns with)
        /// </summary>
        IEnumerable<IPathwayData> GetPathways(bool withReturn = false);

        /// <summary>
        /// What pathways lead to locales
        /// </summary>
        IEnumerable<IPathwayData> GetLocalePathways(bool withReturn = false);

        /// <summary>
        /// What pathways lead to Zones
        /// </summary>
        IEnumerable<IPathwayData> GetZonePathways(bool withReturn = false);
    }
}
