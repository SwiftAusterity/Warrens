using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.Communication.Lexicon;
using NetMud.Data.ConfigData;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Linguistic;
using NutMud.Commands.Attributes;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            //Load the "config" data first
            Backup.ConfigData.LoadEverythingToCache();

            var globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            //We dont move forward without a global config
            if (globalConfig == null)
            {
                globalConfig = new GlobalConfig()
                {
                    Name = "LiveSettings",
                    WebsocketPortalActive = true
                };

                globalConfig.SystemSave();
            }

            //Load structural data next
            BackingData.LoadEverythingToCache();

            var hotBack = new HotBackup();

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            //Hoover up all the verbs from commands that someone might have coded
            ProcessSystemVerbs();

            var gossipServer = new Gossip.GossipClient();
            Task.Run(() => gossipServer.Launch());

            Func<bool> backupFunction = hotBack.WriteLiveBackup;
            Func<bool> backingDataBackupFunction = BackingData.WriteFullBackup;

            //every 15 minutes after half an hour
            Processor.StartSingeltonChainedLoop("HotBackup", 30 * 60, 15 * 60, -1, backupFunction);

            //every 2 hours after 1 hour
            Processor.StartSingeltonChainedLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);
        }

        private static void ProcessSystemVerbs()
        {
            var commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
            var loadedCommands = commandsAssembly.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(ICommand)));

            foreach (var comm in loadedCommands)
            {
                var commandVerbs = comm.GetCustomAttributes<CommandKeywordAttribute>().Where(att => !att.PreventBecomingAVerb).Select(att => att.Keyword);

                foreach (var verb in commandVerbs)
                    LexicalProcessor.VerifyDictata(new Dictata() { WordType = LexicalType.Verb, Name = verb, Elegance = 1, Severity = 1, Quality = 1, Tense = LexicalTense.Present });
            }

        }
    }
}