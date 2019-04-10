using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Entity for Rooms
    /// </summary>
    public interface IRoom : IRoomFramework, IActor, ILocation, ISpawnAsSingleton<IRoom>
    {

    }
}
