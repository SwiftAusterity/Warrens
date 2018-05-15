using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("drop", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IEntity), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Drop : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Drop()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            var thing = (IEntity)Subject;
            var actor = (IContains)Actor;
            IContains place = (IContains)OriginLocation;

            actor.MoveFrom(thing);
            place.MoveInto(thing);

            sb.Add("You drop $S$.");

            var toActor = new Message(MessagingType.Visible, 1)
            {
                Override = sb
            };

            var toOrigin = new Message(MessagingType.Visible, 30)
            {
                Override = new string[] { "$A$ drops $S$." }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, thing, null, OriginLocation.CurrentLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: drop &lt;object&gt;");

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Drop moves an object from your inventory to the room you are currently in.");
            }
            set { }
        }
    }
}
