using System.Collections.Generic;

namespace NetMud.DataStructure.Behaviors.Rendering
{
    public interface IHelpful
    {
        IEnumerable<string> RenderHelpBody();
    }
}
