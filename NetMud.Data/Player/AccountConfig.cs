using NetMud.Data.Architectural;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Web.Script.Serialization;

namespace NetMud.Data.Players
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    [Serializable]
    [IgnoreAutomatedBackup]
    public class AccountConfig : ConfigData, IAccountConfig
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType => ContentApprovalType.None;

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ConfigDataType Type => ConfigDataType.Player;

        [ScriptIgnore]
        [JsonIgnore]
        private IAccount _account { get; set; }

        /// <summary>
        /// Account data object this is owned by
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IAccount Account
        {
            get
            {
                if (_account == null && !string.IsNullOrWhiteSpace(Name))
                {
                    _account = Players.Account.GetByHandle(Name);
                }

                return _account;
            }
        }


        /// <summary>
        /// Does someone see chatter from the Gossip network?
        /// </summary>
        [Display(Name = "Gossip Enabled", Description = "Toggle whether or not you see chat coming from the InterMUD Gossip Network.")]
        [UIHint("Boolean")]
        public bool GossipSubscriber { get; set; }

        [JsonConstructor]
        public AccountConfig()
        {
        }

        public AccountConfig(IAccount account)
        {
            _account = account;


            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = _account.GlobalIdentityHandle;
            }

            GossipSubscriber = true;

        }

        public bool RestoreConfig(IAccount account)
        {
            if (account == null)
            {
                return false;
            }

            if (_account == null)
            {
                _account = account;
            }

            DataAccess.FileSystem.ConfigData configData = new DataAccess.FileSystem.ConfigData();

            string directory = configData.GetCurrentDirectoryForEntity(this);

            DirectoryInfo charDirectory = new DirectoryInfo(directory);
            IAccountConfig newConfig = null;

            try
            {
                FileInfo file = charDirectory.EnumerateFiles("*.AccountConfig", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (file == null)
                {
                    return false;
                }

                newConfig = (IAccountConfig)configData.ReadEntity(file, GetType());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //Let it keep going
            }

            if (newConfig != null)
            {
                GossipSubscriber = newConfig.GossipSubscriber;

                ConfigDataCache.Add(this);

                return true;
            }

            return false;
        }

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new AccountConfig
            {
                Name = Name,
                GossipSubscriber = GossipSubscriber,
            };
        }
    }
}
