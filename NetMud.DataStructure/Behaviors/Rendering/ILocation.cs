using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    public interface ILocation : IEntity, IRendersLocation
    {
        string MoveTo<T>(T thing);
        string MoveTo<T>(T thing, string containerName);
        string MoveFrom<T>(T thing);
        string MoveFrom<T>(T thing, string containerName);
    }
}
