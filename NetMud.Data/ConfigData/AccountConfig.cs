using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web.Script.Serialization;

namespace NetMud.Data.ConfigData
{
    /// <summary>
    /// The account configuration for a player
    /// </summary>
    [Serializable]
    public class AccountConfig : ConfigData, IAccountConfig
    {
        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        public override ConfigDataType Type { get { return ConfigDataType.Player; } }

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
                    _account = System.Account.GetByHandle(Name);

                return _account;
            }
        }

        /// <summary>
        /// Whether or not the person wants the tutorial tooltips on; false = off
        /// </summary>
        public bool UITutorialMode { get; set; }

        /// <summary>
        /// The ui config for this player
        /// </summary>
        public IModularUIConfig UIConfig { get; set; }

        public AccountConfig()
        {

        }

        public AccountConfig(IAccount account)
        {
            _account = account;

            if (string.IsNullOrWhiteSpace(Name))
                Name = account.GlobalIdentityHandle;
        }

        public bool RestoreConfig()
        {
            if (Account == null)
                return false;

            var configData = new DataAccess.FileSystem.ConfigData();

            var directory = configData.GetCurrentDirectoryForEntity(this);

            if (!configData.VerifyDirectory(directory, false))
            {
                LoggingUtility.LogError(new AccessViolationException(string.Format("Current directory for account config data {0} does not exist.", Name)));
                return false;
            }

            var file = new FileInfo(directory + configData.GetEntityFilename(this));
            IAccountConfig newConfig = (IAccountConfig)configData.ReadEntity(file, GetType());

            UIConfig = newConfig.UIConfig;
            UITutorialMode = newConfig.UITutorialMode;

            ConfigDataCache.Add(this);

            return true;
        }
    }
}
