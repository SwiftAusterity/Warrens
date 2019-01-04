using NetMud.Cartography;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Rendering
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("look", false, "l")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ILookable), CacheReferenceType.Entity, true)]
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

                //Send a full map refresh
                if (Actor.ImplementsType<IPlayer>())
                    Utilities.SendMapToPlayer((IPlayer)Actor);

                return;
            }

            ILookable lookTarget = (ILookable)Subject;

            Message toActor = new Message()
            {
                Body = sb
            };

            Message toOrigin = new Message()
            {
                Body = new string[] { "$A$ looks at $T$." }
            };

            Message toSubject = new Message()
            {
                Body = new string[] { "$A$ looks at $T$." }
            };

            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToSubject = new List<IMessage> { toSubject }
            };

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentZone, null);
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
