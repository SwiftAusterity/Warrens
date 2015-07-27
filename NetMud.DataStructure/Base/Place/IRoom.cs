using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IRoom : IActor, ILocation, ISpawnAsSingleton
    {
        IEntityContainer<IInanimate> ObjectsInRoom { get; set; }
        IEntityContainer<IMobile> MobilesInRoom { get; set; }
        IEntityContainer<IPathway> Pathways { get; set; }
    }
}
