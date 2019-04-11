using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    public interface IContext
    {
        /// <summary>
        /// The original input we recieved to parse
        /// </summary>
        string OriginalCommandString { get; }

        /// <summary>
        /// The entity invoking the command
        /// </summary>
        IActor Actor { get; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        object Subject { get; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        object Target { get; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        object Supporting { get; }

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        IGlobalPosition Position { get; }

        /// <summary>
        /// The command (method) we found after parsing
        /// </summary>
        ICommand Command { get; }

        /// <summary>
        /// Rolling list of errors encountered during parsing
        /// </summary>
        List<string> AccessErrors { get; }
    }
}
