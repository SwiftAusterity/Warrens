using NetMud.Data.System;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.LookupData
{
    [Serializable]
    [IgnoreAutomatedBackup]
    public class World : BackingDataPartial, IWorld
    {
        public IMap WorldMap { get; private set; }
    }
}
