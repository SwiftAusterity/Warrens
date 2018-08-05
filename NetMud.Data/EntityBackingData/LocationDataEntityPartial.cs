using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.Rendering;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.EntityBackingData
{
    public abstract class LocationDataEntityPartial : EntityBackingDataPartial, ILocationData
    {
        public abstract ILocation GetLiveInstance();

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>
        public virtual IEnumerable<IPathwayData> GetPathways(bool withReturn = false)
        {
            return BackingDataCache.GetAll<IPathwayData>().Where(path => path.Origin.Equals(this) || (withReturn && path.Destination.Equals(this)));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>       
        public IEnumerable<IPathwayData> GetLocalePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IRoomData)));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>      
        public IEnumerable<IPathwayData> GetZonePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IZoneData)));
        }
    }
}
