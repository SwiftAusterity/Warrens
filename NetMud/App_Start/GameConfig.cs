using NetMud.Backup;
using NetMud.CentralControl;
using System;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            //Load the "referential" data first
            ConfigData.LoadEverythingToCache();
            BackingData.LoadEverythingToCache();

            var hotBack = new HotBackup();

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            var customSockServer = new Websock.Server();
            customSockServer.Launch(2929);

            var gossipServer = new Gossip.GossipClient();
            gossipServer.Launch();

            Func<bool> backupFunction = hotBack.WriteLiveBackup;
            Func<bool> backingDataBackupFunction = BackingData.WriteFullBackup;

            //every 15 minutes after half an hour
            Processor.StartSingeltonChainedLoop("HotBackup", 30 * 60, 15 * 60, -1, backupFunction);

            //every 2 hours after 1 hour
            Processor.StartSingeltonChainedLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);
        }
    }
}