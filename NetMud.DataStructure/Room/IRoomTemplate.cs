using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Room
{
    /// <summary>
    /// Backing data for Rooms
    /// </summary>
    public interface IRoomTemplate : IRoomFramework, ILocationData, ISingleton<IRoom>
    {
    }
}
