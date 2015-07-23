using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IInanimate : IActor, ILocation, ISpawnAsMultiple
    {
        EntityContainer<IInanimate> Contents { get; set; }

        long LastKnownLocation { get; set; }
        string LastKnownLocationType { get; set; }
    }
}
