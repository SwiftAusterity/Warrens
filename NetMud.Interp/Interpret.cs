using System;
using System.Linq;

using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.Utility;
using NetMud.DataAccess;

namespace NetMud.Interp
{
    public static class Interpret
    {
        public static string Render(string commandString, IActor actor)
        {
            try
            {
                var commandContext = new Context(commandString, actor);

                //Derp, we had an error with accessing the command somehow, usually to do with parameter collection or access permissions
                if (commandContext.AccessErrors.Count() > 0)
                    return RenderUtility.EncapsulateOutput(commandContext.AccessErrors);

                commandContext.Command.Execute();
            }
            catch(Exception ex)
            {
                //TODO: Dont return this sort of thing, testing phase only
                LoggingUtility.LogError(ex);
                return ex.Message;
            }

            return string.Empty;
        }
    }
}
