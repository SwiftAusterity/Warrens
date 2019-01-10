using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Room;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Data.Architectural.EntityBase
{
    public abstract class LocationTemplateEntityPartial : EntityTemplatePartial, ILocationData
    {
        public abstract ILocation GetLiveInstance();

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>
        public virtual IEnumerable<IPathwayTemplate> GetPathways(bool withReturn = false)
        {
            return TemplateCache.GetAll<IPathwayTemplate>().Where(path => path.Origin.Equals(this) || (withReturn && path.Destination.Equals(this)));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>       
        public IEnumerable<IPathwayTemplate> GetLocalePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IRoomTemplate)));
        }

        /// <summary>
        /// What pathways are affiliated with this room data (what it spawns with)
        /// </summary>
        /// <param name="withReturn">includes paths into this room as well</param>
        /// <returns>the valid pathways</returns>      
        public IEnumerable<IPathwayTemplate> GetZonePathways(bool withReturn = false)
        {
            return GetPathways(withReturn).Where(path => path.Destination.GetType().GetInterfaces().Contains(typeof(IRoomTemplate)));
        }
    }
}
