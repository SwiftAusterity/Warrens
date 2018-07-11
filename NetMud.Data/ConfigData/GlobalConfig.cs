using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.System;
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
    }
}
