using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Player character + account entity class
    /// </summary>
    public interface IPlayer : IEntity, IPlayerFramework, ISpawnAsSingleton<IPlayer>
    {
    }
}
