using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System.Collections.Generic;

namespace NetMud.Commands.Attributes
{
    public abstract class CommandPartial : ICommand, IHelpful
    {
        public abstract void Execute();

        public abstract IEnumerable<string> RenderSyntaxHelp();

        /// <summary>
        /// The entity invoking the command
        /// </summary>
        public IActor Actor { get; set; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        public object Subject { get; set; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        public object Supporting { get; set; }

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        public ILocation OriginLocation { get; set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; set; }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public abstract string HelpText { get; set; }

        public virtual IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }
    }
}
