using NetMud.DataAccess;
using NetMud.LiveData;
using System.Threading;
using System.Web.Hosting;
using System.Web.Http;

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

            //Rooms, paths, spawns (objs then mobs)
            Communication.RegisterActiveService(Websock.Server.StartServer("localhost", 2929), 2929);

            var newToken = new CancellationTokenSource();
            newToken.CancelAfter(60 * 30 * 1000);

            hotBack.LoopHotbackup(60 * 10, newToken.Token, 60 * 30 * 1000);
        }
    }
}