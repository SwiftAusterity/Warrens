using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Zone;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// Config settings for how the personal maps get spawned
    /// </summary>
    [Serializable]
    public class PersonalZoneConfig : IPersonalZoneConfig
    {
        [JsonProperty("Basis")]
        public TemplateCacheKey _basis { get; set; }

        /// <summary>
        /// The zone data to base this one on
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IZoneTemplate Basis
        {
            get
            {
                if (_basis == null)
                    return null;

                return TemplateCache.Get<IZoneTemplate>(_basis);
            }
            set
            {
                if (value == null)
                    return;

                _basis = new TemplateCacheKey(value);
            }
        }

        /// <summary>
        /// Random animals to put in
        /// </summary>
        public HashSet<INPCRepop> WildAnimals { get; set; }

        public PersonalZoneConfig()
        {
            WildAnimals = new HashSet<INPCRepop>();
        }
    }
}
