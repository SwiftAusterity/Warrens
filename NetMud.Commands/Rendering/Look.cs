using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Data.System;
using System;

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
            var sb = new List<string>();

            //Just do a blank execution as the channel will handle doing the room updates
            if (Subject == null)
            {
                //sb.AddRange(OriginLocation.CurrentLocation.RenderToLook(Actor));

                var blankMessenger = new MessageCluster(new Message(MessagingType.Visible, new Occurrence() { Strength = 999 }) { Override = new string[] { "You observe your surroundings." } });

                blankMessenger.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentLocation, null);
                return;
            }
            else
            {
                var lookTarget = (ILookable)Subject;
                sb.AddRange(lookTarget.RenderToLook(Actor));
            }

            var toActor = new Message(MessagingType.Visible, new Occurrence() { Strength = 999 })
            {
                Override = sb
            };

            var toOrigin = new Message(MessagingType.Visible, new Occurrence() { Strength = 5 })
            {
                Override = new string[] { "$A$ looks at $T$." }
            };

            var toSubject = new Message(MessagingType.Visible, new Occurrence() { Strength = 1 })
            {
                Override = new string[] { "$A$ looks at $T$." }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToSubject = new List<IMessage> { toSubject }
            };

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation.CurrentLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: look",
                "look &lt;target&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Look provides useful information about the location you are in or a target object or mobile.");
            }
            set { }
        }
    }
}
