using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface INonPlayerCharacter : IEntityBackingData, IGender
    {
        string SurName { get; set; }
        string FullName();
    }
}
