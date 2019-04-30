using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.Data.Gossip;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Gossip;
using System;
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

            IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));
            HttpApplication instance = HttpContext.Current.ApplicationInstance;
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
                    .Where(player => player.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
                    .Select(player => new Member()
                    {
                        Name = player.AccountHandle,
                        WriteTo = (message) => player.WriteTo(new string[] { message })
                    }).ToArray();

                void exceptionLogger(Exception ex) => LoggingUtility.LogError(ex);
                void activityLogger(string message) => LoggingUtility.Log(message, LogChannels.GossipServer);

                GossipClient gossipServer = new GossipClient(gossipConfig, exceptionLogger, activityLogger, playerList);

                Task.Run(() => gossipServer.Launch());

                LiveCache.Add(gossipServer, "GossipWebClient");
            }

            Func<bool> backupFunction = hotBack.WriteLiveBackup;
            Func<bool> backingDataBackupFunction = Templates.WriteFullBackup;

            //every 30 minutes after half an hour
            Processor.StartSingeltonChainedLoop("HotBackup", 30 * 60, 30 * 60, -1, backupFunction);

            //every 2 hours after 1 hour
            Processor.StartSingeltonChainedLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);
        }
    }
}