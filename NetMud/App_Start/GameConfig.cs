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
            var hotBack = new HotBackup();

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            var customSockServer = new NetMud.Websock.Server();
            customSockServer.Launch(2929);

            Func<bool> backupFunction = hotBack.WriteLiveBackup;

            Processor.StartNewLoop("HotBackup", 30 * 60, 5 * 60, 1800, backupFunction);
        }
    }
}