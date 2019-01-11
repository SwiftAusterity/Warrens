using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "Account Name", Description = "Account (or gossip user) name for the new acquaintence")]
        [DataType(DataType.Text)]
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
        [Display(Name = "Friend?", Description = "Is this a friend. On = friend, Off = block user")]
        [UIHint("Boolean")]
        public bool IsFriend { get; set; }

        /// <summary>
        /// Is this person a gossip account? the Person will be null then.
        /// </summary>
        [Display(Name = "Gossip User", Description = "Is this person an external user coming from the InterMUD Gossip Network.")]
        [UIHint("Boolean")]
        public bool GossipSystem { get; set; }

        /// <summary>
        /// What notifications are you subscribed to for this person
        /// </summary>
        [Display(Name = "Notifications", Description = "Events you want to be notified about concerning this person.")]
        [DataType(DataType.Text)]
        public AcquaintenceNotifications[] NotificationSubscriptions { get; set; }
    }
}
