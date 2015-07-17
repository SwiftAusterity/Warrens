using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.Place
{
    public interface IPath : IActor
    {
        IRoom ToRoom { get; set; }
        IRoom FromRoom { get; set; }

        MessageCluster Enter { get; set; }
    }
}
