using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;
using NetMud.Data.Game;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Data.System;

namespace NutMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("gotozone", false, true, true)]
    [CommandPermission(StaffRank.Guest)]
    [CommandParameter(CommandUsage.Subject, typeof(Zone), new CacheReferenceType[] { CacheReferenceType.Entity }, false)] //for names
    [CommandRange(CommandRangeType.Global, 0)]
    public class GotoZone : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public GotoZone()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var moveTo = (ILocation)Subject;
            var sb = new List<string>
            {
                "You teleport."
            };

            var toActor = new Message(MessagingType.Visible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            var toOrigin = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "$A$ disappears in a puff of smoke." }
            };

            var toDest = new Message(MessagingType.Visible, new Occurrence() { Strength = 30 })
            {
                Override = new string[] { "$A$ appears out of nowhere." }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToDestination = new List<IMessage> { toDest }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentLocation, null);

            Actor.TryTeleport(new GlobalPosition(moveTo));
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: goto &lt;room name&gt;"
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
                return string.Format("Goto allows staff members to directly teleport to a room irrespective of its capacity limitations.");
            }
            set { throw new NotImplementedException(); }
        }
    }
}
