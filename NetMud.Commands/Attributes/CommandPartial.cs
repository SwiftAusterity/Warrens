using NetMud.Communication.Messaging;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.Commands.Attributes
{
    public abstract class CommandPartial : ICommand, IHelpful
    {
        public abstract void Execute();

        public abstract IEnumerable<string> RenderSyntaxHelp();

        /// <summary>
        /// The command word originally used to find this command
        /// </summary>
        public string CommandWord { get; set; }

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
        public IGlobalPosition OriginLocation { get; set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; set; }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public abstract MarkdownString HelpText { get; set; }

        public virtual IEnumerable<string> RenderHelpBody()
        {
            List<string> sb = new List<string>
            {
                HelpText
            };

            return sb;
        }

        public virtual void RenderError(string error)
        {
            ILexicalParagraph toActor = new LexicalParagraph(error);

            Message messagingObject = new Message(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }
    }
}
