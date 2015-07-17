using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IObject : IActor, ILocation
    {
        EntityContainer<IObject> Contents { get; set; }

        long LastKnownLocation { get; set; }
        string LastKnownLocationType { get; set; }
    }
}
