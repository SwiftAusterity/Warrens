using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IRoom : IActor, ILocation, ISpawnAsSingleton
    {
        EntityContainer<IInanimate> ObjectsInRoom { get; set; }
        EntityContainer<IMobile> MobilesInRoom { get; set; }
        EntityContainer<IPathway> Pathways { get; set; }
    }
}
