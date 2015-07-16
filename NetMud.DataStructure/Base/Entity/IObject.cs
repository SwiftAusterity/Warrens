using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IObject : IActor, ILocation
    {
        EntityContainer<IObject> Contains { get; set; }
    }
}
