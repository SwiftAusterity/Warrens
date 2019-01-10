using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// player-to-player connections
    /// </summary>
    [Serializable]
    public class Acquaintence : IAcquaintence
    {
        /// <summary>
        /// The account handle of the person involved
        /// </summary>
        public string PersonHandle { get; set; }

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
                if (_person == null && !string.IsNullOrWhiteSpace(PersonHandle))
                    _person = Account.GetByHandle(PersonHandle);

                return _person;
            }
            set
            {
                if (value != null)
                    PersonHandle = value.GlobalIdentityHandle;
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
        public AcquaintenceNotifications[] NotificationSubscriptions { get; set; }
    }
}
