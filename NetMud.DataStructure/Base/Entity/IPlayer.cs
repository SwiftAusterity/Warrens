using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IPlayer : IMobile, ISpawnAsSingleton
    {
        string DescriptorID { get; set; }
        DescriptorType Descriptor { get; set; }
    }

    public enum DescriptorType
    {
        WebSockets
    }
}
