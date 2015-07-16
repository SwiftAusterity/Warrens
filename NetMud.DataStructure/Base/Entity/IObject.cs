using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.Automation;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IObject : IActor, ILocation
    {
        IEntityContainer<IObject> Contains { get; set; }
    }
}
