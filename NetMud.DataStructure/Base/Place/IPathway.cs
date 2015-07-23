
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IPathway : IActor, ISpawnAsSingleton
    {
        ILocation ToLocation { get; set; }
        ILocation FromLocation { get; set; }

        MovementDirectionType MovementDirection { get; }

        MessageCluster Enter { get; set; }
    }
}
