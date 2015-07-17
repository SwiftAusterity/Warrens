using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IRoom : IActor, ILocation, ISpawnAsSingleton
    {
        EntityContainer<IObject> ObjectsInRoom { get; set; }
        EntityContainer<IMobile> MobilesInRoom { get; set; }
    }
}
