using NetMud.Data.Architectural;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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
        /// Whether or not the person wants the tutorial tooltips on; false = off
        /// </summary>
        [Display(Name = "Tutorial Mode", Description = "Toggle the Game Client UI Tutorial mode off to remove tip popups permenantly.")]
        [UIHint("Boolean")]
        public bool UITutorialMode { get; set; }

        /// <summary>
        /// Does someone see chatter from the Gossip network?
        /// </summary>
        [Display(Name = "Gossip Enabled", Description = "Toggle whether or not you see chat coming from the InterMUD Gossip Network.")]
        [UIHint("Boolean")]
        public bool GossipSubscriber { get; set; }

        /// <summary>
        /// Whether or not the person wants foley effects on; true == muted
        /// </summary>
        [Display(Name = "Perma-Mute Sound", Description = "Toggle to mute foley effects in game by default.")]
        [UIHint("Boolean")]
        public bool SoundMuted { get; set; }

        /// <summary>
        /// Whether or not the person wants background music on; true == muted
        /// </summary>
        [Display(Name = "Perma-Mute Music", Description = "Toggle to mute music in game by default.")]
        [UIHint("Boolean")]
        public bool MusicMuted { get; set; }

        /// <summary>
        /// Background music playlists
        /// </summary>
        public HashSet<IPlaylist> Playlists { get; set; }

        /// <summary>
        /// Friends and Foes of this account
        /// </summary>
        public IEnumerable<IAcquaintence> Acquaintences { get; set; }

        [JsonProperty("Notifications")]
        public HashSet<ConfigDataCacheKey> _notifications { get; set; }

        /// <summary>
        /// Messages to this account
        /// </summary>
        [ScriptIgnore]
        [JsonIgnore]
        public IEnumerable<IPlayerMessage> Notifications
        {
            get
            {
                if (_notifications == null)
                {
                    _notifications = new HashSet<ConfigDataCacheKey>();
                }

                return ConfigDataCache.GetMany<IPlayerMessage>(_notifications);
            }
            set
            {
                if (value != null)
                {
                    _notifications = new HashSet<ConfigDataCacheKey>(value.Select(note => new ConfigDataCacheKey(note)));
                }
            }
        }

        [JsonProperty("UIModules")]
        public IList<Tuple<TemplateCacheKey, int>> _UIModules { get; set; }

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
                {
                    _UIModules = new List<Tuple<TemplateCacheKey, int>>();
                }

                return _UIModules.Select(k => new Tuple<IUIModule, int>(TemplateCache.Get<IUIModule>(k.Item1), k.Item2));
            }
            set
            {
                if (value == null)
                {
                    return;
                }

                _UIModules = value.Select(k => new Tuple<TemplateCacheKey, int>(new TemplateCacheKey(k.Item1), k.Item2)).ToList();
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
            Acquaintences = new List<IAcquaintence>();
            Notifications = new List<IPlayerMessage>();
            Playlists = new HashSet<IPlaylist>();
            UIModules = Enumerable.Empty<Tuple<IUIModule, int>>();

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            if(globalConfig != null)
            {
                UILanguage = globalConfig.BaseLanguage;
            }
        }

        public AccountConfig(IAccount account)
        {
            _account = account;

            Acquaintences = new List<IAcquaintence>();
            Notifications = new List<IPlayerMessage>();
            Playlists = new HashSet<IPlaylist>();
            UIModules = Enumerable.Empty<Tuple<IUIModule, int>>();

            if (string.IsNullOrWhiteSpace(Name))
            {
                Name = _account.GlobalIdentityHandle;
            }

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            if (globalConfig != null)
            {
                UILanguage = globalConfig.BaseLanguage;
            }

            UITutorialMode = true;
            MusicMuted = true;
            GossipSubscriber = true;

        }

        /// <summary>
        /// Does this person want this notification
        /// </summary>
        /// <param name="playerName">The player's name who's triggering the notification</param>
        /// <param name="isGossipSystem">Is this the gossip system</param>
        /// <param name="type">what type of notification is this</param>
        /// <returns>Whether or not they want it</returns>
        public bool WantsNotification(string playerName, bool isGossipSystem, AcquaintenceNotifications type)
        {
            return Acquaintences.Any(acq => acq.IsFriend
                                                && acq.PersonHandle.Equals(playerName, StringComparison.InvariantCultureIgnoreCase)
                                                && acq.NotificationSubscriptions.Contains(type)
                                                && isGossipSystem == acq.GossipSystem);
        }

        /// <summary>
        /// Does this person want this notification
        /// </summary>
        /// <param name="playerName">The player's name who's triggering the notification</param>
        /// <param name="isGossipSystem">Is this the gossip system</param>
        /// <param name="type">what type of notification is this</param>
        /// <returns>Whether or not they want it</returns>
        public bool IsBlocking(string playerName, bool isGossipSystem)
        {
            return Acquaintences.Any(acq => !acq.IsFriend
                                                && acq.PersonHandle.Equals(playerName, StringComparison.InvariantCultureIgnoreCase)
                                                && isGossipSystem == acq.GossipSystem);
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
                UITutorialMode = newConfig.UITutorialMode;
                GossipSubscriber = newConfig.GossipSubscriber;
                SoundMuted = newConfig.SoundMuted;
                MusicMuted = newConfig.MusicMuted;
                UILanguage = newConfig.UILanguage;

                GetNotifications(configData, charDirectory);

                if (newConfig.Playlists == null)
                {
                    Playlists = new HashSet<IPlaylist>();
                }
                else
                {
                    Playlists = newConfig.Playlists;
                }

                if (newConfig.Acquaintences == null)
                {
                    Acquaintences = Enumerable.Empty<IAcquaintence>();
                }
                else
                {
                    Acquaintences = newConfig.Acquaintences;
                }

                if (newConfig.UIModules == null)
                {
                    UIModules = Enumerable.Empty<Tuple<IUIModule, int>>();
                }
                else
                {
                    UIModules = newConfig.UIModules;
                }

                ConfigDataCache.Add(this);

                return true;
            }

            return false;
        }

        private void GetNotifications(DataAccess.FileSystem.ConfigData dataAccessor, DirectoryInfo charDirectory)
        {
            try
            {
                IEnumerable<FileInfo> files = charDirectory.EnumerateFiles("*.PlayerMessage", SearchOption.TopDirectoryOnly);

                List<IPlayerMessage> dataList = new List<IPlayerMessage>();
                foreach (FileInfo file in files)
                {
                    if (file == null)
                    {
                        continue;
                    }

                    IPlayerMessage newMessage = (IPlayerMessage)dataAccessor.ReadEntity(file, typeof(IPlayerMessage));

                    if (newMessage != null)
                    {
                        ConfigDataCache.Add(newMessage);
                        dataList.Add(newMessage);
                    }
                }

                Notifications = dataList;
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                //Let it keep going
            }
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
                Acquaintences = Acquaintences,
                GossipSubscriber = GossipSubscriber,
                MusicMuted = MusicMuted,
                Notifications = Notifications,
                Playlists = Playlists,
                SoundMuted = SoundMuted,
                UITutorialMode = UITutorialMode
            };
        }
    }
}
