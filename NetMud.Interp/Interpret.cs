using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NetMud.Data.Behaviors.Rendering;
using NetMud.Utility;

namespace NetMud.Interp
{
    public static class Interpret
    {
        public static string Render(string commandString, IActor actor)
        {
            //Need some way to build a context object to work off of
            //TODO: Actually care about actor details somehow, off of ICommand likely
            var commandContext = new Context(commandString, actor);

            //Derp, we had an error with accessing the command somehow, usually to do with parameter collection or access permissions
            if (commandContext.AccessErrors.Count() > 0)
                return RenderUtility.EncapsulateOutput(commandContext.AccessErrors);

            return RenderUtility.EncapsulateOutput(commandContext.Command.Execute());
        }
    }
}
