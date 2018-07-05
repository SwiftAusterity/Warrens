using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// Messages to players
    /// </summary>
    [Serializable]
    public class PlayerMessage : ConfigData, IPlayerMessage
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

        /// <summary>
        /// The body of the message
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        public string Subject { get; set; }

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _sender { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IAccount Sender
        {
            get
            {
                //Name = SenderHandle
                if (_sender == null && !string.IsNullOrWhiteSpace(Name))
                    _sender = System.Account.GetByHandle(Name);

                return _sender;
            }
            set
            {
                if (value != null)
                    Name = value.GlobalIdentityHandle;
            }
        }

        /// <summary>
        /// Name of the recipient character (can be blank)
        /// </summary>
        public string RecipientName { get; set; }

        /// <summary>
        /// Recipeint character
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public ICharacter Recipient
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RecipientName))
                    return null;

                var characters = PlayerDataCache.GetAllForAccountHandle(Name);

                //TODO: Maybe get a character by name in cache
                return characters.FirstOrDefault(ch => ch.Name.Equals(RecipientName,  StringComparison.InvariantCultureIgnoreCase));
            }
            set
            {
                if (value != null)
                    RecipientName = value.Name;
            }
        }

        /// <summary>
        /// Is this important? Does it make the UI bell ring
        /// </summary>
        public bool Important { get; set; }
    }
}
