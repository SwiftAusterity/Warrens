using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Entity
{
    public interface IIntelligence : IMobile, ISpawnAsMultiple
    {
        EntityContainer<IObject> Inventory { get; set; }
    }
}
