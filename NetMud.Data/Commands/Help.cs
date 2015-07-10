using NetMud.Data.Base.System;
using NetMud.Data.Behaviors.Rendering;
using NetMud.Data.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Commands
{
    [CommandKeyword("Help")]
    class Help : ICommand, IHelpful
    {
    }
}
