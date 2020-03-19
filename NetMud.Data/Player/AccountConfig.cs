using NetMud.Data.Architectural;
using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
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
        /// The UI language for output purposes
        /// </summary>
        [JsonProperty("UILanguage")]
        private ConfigDataCacheKey _uiLanguage { get; set; }

        /// <summary>
        /// The UI language for output purposes
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        [Display(Name = "Game UI Language", Description = "The language the game will output to you while playing.")]
        [UIHint("LanguageList")]
        [LanguageDataBinder]
        public ILanguage UILanguage
        {
            get
            {
                if (_uiLanguage == null)
                {
                    return null;
                }

                return ConfigDataCache.Get<ILanguage>(_uiLanguage);
            }
            set
            {
                if (value == null)
                {
                    _uiLanguage = null;
                    return;
                }

                _uiLanguage = new ConfigDataCacheKey(value);
            }
        }

        [JsonConstructor]
        public AccountConfig()
        {
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            if(globalConfig != null)
            {
                UILanguage = globalConfig.BaseLanguage;
            }
        }

        public AccountConfig(IAccount account)
        {
            _account = account;

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = _account.GlobalIdentityHandle;
            }

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            if (globalConfig != null)
            {
                UILanguage = globalConfig.BaseLanguage;
            }

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
                UILanguage = newConfig.UILanguage;

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
            };
        }
    }
}
