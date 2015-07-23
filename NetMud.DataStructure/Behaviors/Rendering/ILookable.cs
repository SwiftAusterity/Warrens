using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    public interface ILookable
    {
        IEnumerable<string> RenderToLook();
    }
}
