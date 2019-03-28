using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.Commands.Attributes;
using NetMud.Communication.Lexical;
using NetMud.Data.Gossip;
using NetMud.Data.Linguistic;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Gossip;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            //Load the "config" data first
            ConfigData.LoadEverythingToCache();

            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            //We dont move forward without a global config
            if (globalConfig == null)
            {
                globalConfig = new GlobalConfig();

                globalConfig.SystemSave();
            }

            if (globalConfig.BaseLanguage == null)
            {
                ILanguage baseLanguage = ConfigDataCache.GetAll<ILanguage>().FirstOrDefault();

                if (baseLanguage == null)
                {
                    LoggingUtility.Log("There are no valid languages. Generating new base language.", LogChannels.SystemErrors, true);

                    baseLanguage = new Language()
                    {
                        Name = "English",
                        GoogleLanguageCode = "en-us",
                        AntecendentPunctuation = true,
                        PrecedentPunctuation = false,
                        Gendered = false,
                        UIOnly = true
                    };

                    baseLanguage.SystemSave();
                }

                globalConfig.BaseLanguage = baseLanguage;
                globalConfig.SystemSave();
            }

            LexicalProcessor.LoadWordnet();

            IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));
            var instance = HttpContext.Current.ApplicationInstance;
            Assembly asm = instance.GetType().BaseType.Assembly;
            Version v = asm.GetName().Version;

            //We dont move forward without a global config
            if (gossipConfig == null)
            {
                gossipConfig = new GossipConfig
                {
                    ClientName = "Warrens: White Sands"
                };
            }

            //Update version
            gossipConfig.Version = string.Format(CultureInfo.InvariantCulture, @"{0}.{1}.{2} (r{3})", v.Major, v.Minor, v.Build, v.Revision);
            gossipConfig.SystemSave();

            //Load structural data next
            Templates.LoadEverythingToCache();

            HotBackup hotBack = new HotBackup();

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
            {
                hotBack.NewWorldFallback();
            }

            if (gossipConfig.GossipActive)
            {
                Func<Member[]> playerList = () => LiveCache.GetAll<IPlayer>()
                    .Where(player => player.Descriptor != null && player.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
                    .Select(player => new Member()
                    {
                        Name = player.AccountHandle,
                        WriteTo = (message) => player.WriteTo(new string[] { message }),
                        BlockedMembers = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => !acq.IsFriend).Select(acq => acq.PersonHandle),
                        Friends = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => acq.IsFriend).Select(acq => acq.PersonHandle)
                    }).ToArray();

                void exceptionLogger(Exception ex) => LoggingUtility.LogError(ex);
                void activityLogger(string message) => LoggingUtility.Log(message, LogChannels.GossipServer);

                GossipClient gossipServer = new GossipClient(gossipConfig, exceptionLogger, activityLogger, playerList);

                Task.Run(() => gossipServer.Launch());

                LiveCache.Add(gossipServer, "GossipWebClient");
            }

            //Hoover up all the verbs from commands that someone might have coded
            ProcessSystemVerbs(globalConfig.BaseLanguage);

            Func<bool> backupFunction = hotBack.WriteLiveBackup;
            Func<bool> backingDataBackupFunction = Templates.WriteFullBackup;

            //every 30 minutes after half an hour
            Processor.StartSingeltonChainedLoop("HotBackup", 30 * 60, 30 * 60, -1, backupFunction);

            //every 2 hours after 1 hour
            Processor.StartSingeltonChainedLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);
        }

        private static void ProcessSystemVerbs(ILanguage language)
        {
            Assembly commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
            IEnumerable<Type> loadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            foreach (Type comm in loadedCommands)
            {
                IEnumerable<string> commandVerbs = comm.GetCustomAttributes<CommandKeywordAttribute>().Where(att => !att.PreventBecomingAVerb).Select(att => att.Keyword);

                foreach (string verb in commandVerbs)
                {
                    var newVerb = new Dictata()
                    {
                        Name = verb,
                        Determinant = false,
                        Feminine = false,
                        Plural = false,
                        Positional = LexicalPosition.None,
                        Perspective = NarrativePerspective.None,
                        Possessive = false,
                        Tense = LexicalTense.Present,
                        Semantics = new HashSet<string>() { "system_command" },
                        WordTypes = new HashSet<LexicalType>() { LexicalType.Verb },
                        Language = language
                    };

                    LexicalProcessor.VerifyDictata(newVerb);
                }
            }
        }
    }
}