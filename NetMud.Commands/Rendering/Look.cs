using NetMud.Commands.Attributes;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NutMud.Commands.Rendering
{
    /// <summary>
    /// Invokes the current container's RenderToLook
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
        public override void Execute()
        {
            List<string> sb = new List<string>();

            //Just do a blank execution as the channel will handle doing the room updates
            if (Subject == null)
            {
                //sb.AddRange(OriginLocation.CurrentLocation.RenderToLook(Actor));

                ///Need to do like HMR with a simple "update UI" pipeline TODO
                MessageCluster blankMessenger = new MessageCluster(new Message(MessagingType.Visible, new SensoryEvent() { Strength = 999 }) { Override = new string[] { "You observe your surroundings." } });

                blankMessenger.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentRoom, null);
                return;
            }

            ILookable lookTarget = (ILookable)Subject;

            Message toOrigin = new Message(MessagingType.Visible, new SensoryEvent() { Strength = 5 })
            {
                Override = new string[] { "$A$ looks at $T$." }
            };

            Message toSubject = new Message(MessagingType.Visible, new SensoryEvent() { Strength = 1 })
            {
                Override = new string[] { "$A$ looks at $T$." }
            };

            MessageCluster messagingObject = new MessageCluster(lookTarget.RenderToLook(Actor).ToList())
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToSubject = new List<IMessage> { toSubject }
            };

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentRoom, null);
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
                return string.Format("Look provides useful information about the location you are in or a target object or mobile.");
            }
            set { }
        }
    }
}
