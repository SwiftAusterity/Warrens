using NetMud.Data.Architectural;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
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
                    _sender = Account.GetByHandle(SenderName);

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
                    _recipientAccount = Account.GetByHandle(Name);

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
        public IPlayerTemplate Recipient
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RecipientName))
                    return null;

                IPlayerTemplate character = PlayerDataCache.GetCharacterForAccountHandle(Name);

                if (character.Name.Equals(RecipientName, StringComparison.InvariantCultureIgnoreCase))
                    return character;

                return null;
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

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new PlayerMessage
            {
                Name = Name,
                Body = Body,
                Important = Important,
                Read = Read,
                Subject = Subject,
                Sent = Sent,
                SenderName = SenderName,
                Sender = Sender,
                RecipientName = RecipientName,
                RecipientAccount = RecipientAccount,
                Recipient = Recipient
            };
        }
    }
}
