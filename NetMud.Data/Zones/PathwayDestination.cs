using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Zones
{
    /// <summary>
    /// Where pathways lead to
    /// </summary>
    [Serializable]
    public class PathwayDestination : IPathwayDestination
    {
        /// <summary>
        /// The keywords/name for the pathway
        /// </summary>
        public string Name { get; set; }

        [JsonProperty("Destination")]
        private TemplateCacheKey _destination { get; set; }

        /// <summary>
        /// Where this leads to
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IZoneTemplate Destination
        {
            get
            {
                return TemplateCache.Get<IZoneTemplate>(_destination);
            }
            set
            {
                if (value != null)
                    _destination = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// What tile this goes to
        /// </summary>
        public Coordinate Coordinates { get; set; }
    }
}
