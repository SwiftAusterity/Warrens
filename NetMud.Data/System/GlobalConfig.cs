
using NetMud.Data.Architectural;
using NetMud.Data.Players;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.System
{
    [Serializable]
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

        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        public bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        public bool AdminsOnly { get; set; }

        /// <summary>
        /// Config to handle player death
        /// </summary>
        public IDeathConfig DeathSettings { get; set; }

        public GlobalConfig()
        {
            DeathSettings = new DeathConfig();

            Name = "LiveSettings";
            WebsocketPortalActive = true;
            AdminsOnly = false;
            UserCreationActive = true;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new GlobalConfig
            {
                Name = Name,
                WebsocketPortalActive = WebsocketPortalActive,
                UserCreationActive = UserCreationActive,
                AdminsOnly = AdminsOnly,
                DeathSettings = DeathSettings,
            };
        }
    }
}
