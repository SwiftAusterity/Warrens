using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.Place
{
    public interface IRoom : IActor, ILocation, IData
    {
        string Title { get; set; }

        IEntityContainer<IObject> ObjectsInRoom { get; set; }
        IEntityContainer<IMobile> MobilesInRoom { get; set; }
    }
}
