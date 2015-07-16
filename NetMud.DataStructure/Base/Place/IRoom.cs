using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Automation;

namespace NetMud.DataStructure.Base.Place
{
    public interface IRoom : IActor, ILocation, IData
    {
        string Title { get; set; }

        IEntityContainer<IObject> ObjectsInRoom { get; set; }
        IEntityContainer<IMobile> MobilesInRoom { get; set; }
    }
}
