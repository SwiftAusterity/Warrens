using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IPlayer : IMobile, ISpawnAsSingleton
    {
        string DescriptorID { get; set; }
        DescriptorType Descriptor { get; set; }

        EntityContainer<IObject> Inventory { get; set; }
    }

    public enum DescriptorType
    {
        WebSockets
    }
}
