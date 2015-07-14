using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface ICommand
    {
        IEnumerable<string> Execute();

        IEnumerable<string> RenderSyntaxHelp();
    }
}
