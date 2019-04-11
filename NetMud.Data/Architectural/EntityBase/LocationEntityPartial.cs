using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural.EntityBase;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Partial template for live Location entities
    /// </summary>
    public abstract class LocationEntityPartial : EntityPartial, ILocation
    {
        /// <summary>
        /// The name of the object in the data template
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string TemplateName
        {
            get
            {
                return Template<ILocationData>()?.Name;
            }
        }

        /// <summary>
        /// The backing data for this entity
        /// </summary>
        public override T Template<T>()
        {
            return (T)TemplateCache.Get(new TemplateCacheKey(typeof(ILocationData), TemplateId));
        }

        /// <summary>
        /// current maximum section
        /// </summary>
        public ulong MaxSection { get; set; }
    }
}
