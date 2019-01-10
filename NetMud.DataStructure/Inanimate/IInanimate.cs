using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;

namespace NetMud.DataStructure.Inanimate
{
    /// <summary>
    /// "Object" entity
    /// </summary>
    public interface IInanimate : IActor, IInanimateFramework, ICanBeWorn, ICanBeHeld, IContains, ISpawnAsMultiple
    {
    }
}
