using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface ICharacter : IEntityBackingData
    {
        string SurName { get; set; }
        string AccountHandle { get; set; }
        IAccount Account { get; }
        long LastKnownLocation { get; set; }
        string LastKnownLocationType { get; set; }
        string FullName();
    }
}
