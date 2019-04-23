using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Utility;
using System.Collections.Generic;

namespace NutMud.Commands.Rendering
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandKeyword("look", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ILookable), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Look : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Look()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            List<string> sb = new List<string>();

            //Just do a blank execution as the channel will handle doing the room updates
            if (Subject == null)
            {
                ///Need to do like HMR with a simple "update UI" pipeline TODO
                Message blankMessenger = new Message("You observe your surroundings.");

                blankMessenger.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation, null, 0);
                return true;
            }

            ILookable lookTarget = (ILookable)Subject;

            IEnumerable<string> toOrigin = new string[] { "$A$ looks at $T$." };

            IEnumerable<string> toSubject = new string[] { "$A$ looks at $T$." };

            Message messagingObject = new Message(lookTarget.RenderToVisible(Actor))
            {
                ToOrigin = toOrigin,
                ToSubject = toSubject
            };

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation, null, 0);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: look",
                "look &lt;target&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override MarkdownString HelpText
        {
            get
            {
                return string.Format("Look provides useful information about the location you are in or a target player.");
            }
            set { }
        }
    }
}
