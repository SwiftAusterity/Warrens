using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataStructure.Behaviors.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IPath : IActor, ISpawnAsSingleton
    {
        ILocation ToLocation { get; set; }
        ILocation FromLocation { get; set; }

        MessageCluster Enter { get; set; }
    }
}
