using NetMud.LiveData;
using System.Threading;
using System.Threading.Tasks;
namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            var hotBack = new HotBackup(System.Web.Hosting.HostingEnvironment.MapPath("/HotBackup/"));

            //Our live data restore failed, reload the entire world from backing data
            if (!hotBack.RestoreLiveBackup())
                hotBack.NewWorldFallback();

            //Rooms, paths, spawns (objs then mobs)
            Websock.Server.StartServer("localhost", 2929);
        }
    }
}