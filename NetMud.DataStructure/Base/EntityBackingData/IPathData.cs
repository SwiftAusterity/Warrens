using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface IPathData : IEntityBackingData
    {
        long ToRoomID { get; set; }
        long FromRoomID { get; set; }

        string MessageToActor { get; set; }
        string MessageToOrigin { get; set; }
        string MessageToDestination { get; set; }

        string AudibleToSurroundings { get; set; }
        int AudibleStrength { get; set; }

        string VisibleToSurroundings { get; set; }
        int VisibleStrength { get; set; }
    }
}
