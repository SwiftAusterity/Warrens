using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Framework interface for commands
    /// </summary>
    public interface ICommand
    {
        /* 
         * Syntax:
         *      command <subject> <target> <supporting>
         *  Location is derived from context
         *  Surroundings is derived from location
         */

        /// <summary>
        /// The command word originally used to find this command
        /// </summary>
        string CommandWord { get; set; }

        /// <summary>
        /// The original input that spawned this
        /// </summary>
        string OriginalInput { get; set; }

        /// <summary>
        /// Acting entity that issued this command
        /// </summary>
        IActor Actor { get; set; }

        /// <summary>
        /// The subject for the command being issued
        /// </summary>
        object Subject { get; set; }

        /// <summary>
        /// The target for the command being issued
        /// </summary>
        object Target { get; set; }

        /// <summary>
        /// Any supporting entity for the command
        /// </summary>
        object Supporting { get; set; }

        /// <summary>
        /// Location the Actor was in when command was issued
        /// </summary>
        IGlobalPosition OriginLocation { get; set; }

        /// <summary>
        /// The delay this invokes when executing. Initially is "begun" and actually takes affect at the end.
        /// </summary>
        int ExecutionDelay { get; }

        /// <summary>
        /// The delay this invokes after being executed
        /// </summary>
        int CooldownDelay { get; }

        /// <summary>
        /// Send some sort of error to the client
        /// </summary>
        /// <param name="error">The error</param>
        void RenderError(string error);

        /// <summary>
        /// Execute the command's actions
        /// </summary>
        void Execute(Func<string, IActor, bool> nextAction);

        /// <summary>
        /// Renders syntactical help for command parsing
        /// </summary>
        /// <returns>help output</returns>
        IEnumerable<string> RenderSyntaxHelp();
    }
}
