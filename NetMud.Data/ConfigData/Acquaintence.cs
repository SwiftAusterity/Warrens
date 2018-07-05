using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// player-to-player connections
    /// </summary>
    [Serializable]
    public class Acquaintance : ConfigData, IAcquaintance
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None; //Config data defaults to admin

        /// <summary>
        /// Type of configuation data this is
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Player;

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _person { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IAccount Person
        {
            get
            {
                //Name = PersonHandle
                if (_person == null && !string.IsNullOrWhiteSpace(Name))
                    _person = System.Account.GetByHandle(Name);

                return _person;
            }
            set
            {
                if (value != null)
                    Name = value.GlobalIdentityHandle;
            }
        }

        /// <summary>
        /// Is this a friend or foe?
        /// </summary>
        public bool IsFriend { get; set; }

        /// <summary>
        /// Is this person a gossip account? the Person will be null then.
        /// </summary>
        public bool GossipSystem { get; set; }

        /// <summary>
        /// What notifications are you subscribed to for this person
        /// </summary>
        public AcquaintanceNotifications[] NotificationSubscriptions { get; set; }
    }
}
