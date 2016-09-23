using NetMud.Backup;
using System.Web.Hosting;
using NetMud.CentralControl;
using System;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            //Load the "referential" data first
            BackingData.LoadEverythingToCache();
            //BackingData.LoadEverythingToCacheFromDatabase();

            var hotBack = new HotBackup();

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            var customSockServer = new NetMud.Websock.Server();
            customSockServer.Launch(2929);

            Func<bool> backupFunction = hotBack.WriteLiveBackup;
            Func<bool> backingDataBackupFunction = BackingData.WriteFullBackup;

            //every 5 minutes after half an hour
            Processor.StartNewLoop("HotBackup", 30 * 60, 5 * 60, -1, backupFunction);

            //every 2 hours after 1 hour
            Processor.StartNewLoop("BackingDataFullBackup", 60 * 60, 120 * 60, -1, backingDataBackupFunction);
        }
    }
}