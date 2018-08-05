using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.Linguistic;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    public class GlobalConfig : ConfigData, IGlobalConfig
    {
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.GameWorld;

        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        public bool WebsocketPortalActive { get; set; }

        [JsonProperty("SystemLanguage")]
        public ConfigDataCacheKey _systemLanguage { get; set; }

        /// <summary>
        /// The base language for the system
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ILanguage SystemLanguage 
        {
            get
            {
                if (_systemLanguage == null)
                    _systemLanguage = null;

                return ConfigDataCache.Get<ILanguage>(_systemLanguage);
            }
            set
            {
                if (value != null)
                    _systemLanguage = new ConfigDataCacheKey(value);
            }
        }
    }
}
