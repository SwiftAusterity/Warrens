using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
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
        /// The unique key used to identify, store and retrieve data
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override string UniqueKey => string.Format("{0}_{1}_{2}", Name, RecipientName, Sent.ToBinary());

        /// <summary>
        /// The body of the message
        /// </summary>
        public MarkdownString Body { get; set; }

        /// <summary>
        /// Subject of the message
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// When this was sent
        /// </summary>
        public DateTime Sent { get; set; }

        /// <summary>
        /// Name of the recipient character (can be blank)
        /// </summary>
        public string SenderName { get; set; }

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
                if (_sender == null && !string.IsNullOrWhiteSpace(SenderName))
                    _sender = System.Account.GetByHandle(SenderName);

                return _sender;
            }
            set
            {
                if (value != null)
                {
                    SenderName = value.GlobalIdentityHandle;
                    _sender = value;
                }
            }
        }

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _recipientAccount { get; set; }

        //Name = recipientAccountName
        /// <summary>
        /// The account recieving this
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IAccount RecipientAccount
        {
            get
            {
                if (_recipientAccount == null && !string.IsNullOrWhiteSpace(Name))
                    _recipientAccount = System.Account.GetByHandle(Name);

                return _recipientAccount;
            }
            set
            {
                if (value != null)
                {
                    Name = value.GlobalIdentityHandle;
                    _recipientAccount = value;
                }
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

        /// <summary>
        /// Has this been read yet?
        /// </summary>
        public bool Read { get; set; }
    }
}
