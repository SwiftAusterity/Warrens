using System;
using System.Linq;
using NetMud.DataAccess;
using System.Collections.Generic;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.System;

namespace NetMud.Interp
{
    /// <summary>
    /// Main line in for in-game commands to get parsed
    /// </summary>
    public static class Interpret
    {
        /// <summary>
        /// Scour the system to figure out what the entity wants us to do
        /// </summary>
        /// <param name="commandString">the raw unparsed command coming in</param>
        /// <param name="actor">the entity performing the action</param>
        /// <returns>any pre-execution command errors</returns>
        public static IEnumerable<string> Render(string commandString, IActor actor)
        {
            try
            {
                IContext commandContext = new Context(commandString, actor);

                //Derp, we had an error with accessing the command somehow, usually to do with parameter collection or access permissions
                if (commandContext.AccessErrors.Count() > 0)
                {
                    return commandContext.AccessErrors;
                }

                commandContext.Command.Execute();
            }
            catch(Exception ex)
            {
                //TODO: Dont return this sort of thing, testing phase only, should return some sort of randomized error
                LoggingUtility.LogError(ex);

                return new string[] { "SYSTEM ERROR ALEPH PHASE ONLY SHOWN (this was already logged): " + ex.Message };
            }

            return new string[] { };
        }
    }
}
