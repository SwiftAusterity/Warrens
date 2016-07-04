using NetMud.Backup;
using System.Web.Hosting;
using NetMud.CentralControl;
using System;
using NetMud.Websock;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            var hotBack = new HotBackup(HostingEnvironment.MapPath("/HotBackup/"));

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            var webSockServer = new Websock.Server();
            webSockServer.Launch(2929);

            //var customSockServer = new CustomServer();
            //customSockServer.Launch(2929);

            Func<bool> backupFunction = hotBack.WriteLiveBackup;

            Processor.StartNewLoop("HotBackup", 30 * 60, 5 * 60, 1800, backupFunction);
        }
    }
}