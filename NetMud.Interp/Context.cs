using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.Data.Behaviors.Rendering;
using NetMud.Data.Base.System;

namespace NetMud.Interp
{
    public class Context
    {
        public string OriginalCommandString { get; private set; }
        public IActor Actor { get; private set; }
        public ICommand Command { get; private set; }
        public IEntity Subject { get; private set; }
        public ILocation Location { get; private set; }
        public IEnumerable<ILocation> Surroundings { get; private set; }

        public Context(string fullCommand, IActor actor)
        {
            OriginalCommandString = fullCommand;
            Actor = actor;
        }
    }
}
