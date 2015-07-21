using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;

namespace NetMud
{
    public class GameConfig
    {
        public static void PreloadSupportingEntities()
        {
            var liveWorld = new LiveCache();

            //Rooms, paths, spawns (objs then mobs)
            liveWorld.PreLoadAll<RoomData>();
            liveWorld.PreLoadAll<PathData>();

            Websock.Server.StartServer("localhost", 2929);
        }
    }
}