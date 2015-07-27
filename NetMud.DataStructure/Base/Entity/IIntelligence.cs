using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IIntelligence : IMobile, ISpawnAsMultiple
    {
        IEntityContainer<IInanimate> Inventory { get; set; }
    }
}
