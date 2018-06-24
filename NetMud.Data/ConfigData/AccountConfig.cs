using NetMud.Data.Serialization;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        /// What type of approval is necessary for this content
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.None; } }

        /// <summary>
        /// The type of data this is (for storage)
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
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

        [JsonProperty("UIModules")]
        public IList<Tuple<BackingDataCacheKey, int>> _UIModules { get; set; }

        /// <summary>
        /// The modules to load. Module, quadrant
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<Tuple<IUIModule, int>> UIModules
        {
            get
            {
                if (_UIModules == null)
                    _UIModules = new List<Tuple<BackingDataCacheKey, int>>();

                return _UIModules.Select(k => new Tuple<IUIModule, int>(BackingDataCache.Get<IUIModule>(k.Item1), k.Item2));
            }
            set
            {
                if (value == null)
                    return;

                _UIModules = value.Select(k => new Tuple<BackingDataCacheKey, int>(new BackingDataCacheKey(k.Item1), k.Item2)).ToList();
            }
        }

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

            var charDirectory = new DirectoryInfo(directory);
            IAccountConfig newConfig = null;

            try
            {
                var file = charDirectory.EnumerateFiles("*.AccountConfig", SearchOption.TopDirectoryOnly).FirstOrDefault();

                if (file == null)
                    return false;

                newConfig = (IAccountConfig)configData.ReadEntity(file, GetType());
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //Let it keep going
            }

            if (newConfig != null)
            {
                UIModules = newConfig.UIModules;
                UITutorialMode = newConfig.UITutorialMode;

                ConfigDataCache.Add(this);

                return true;
            }

            return false;
        }
    }
}
