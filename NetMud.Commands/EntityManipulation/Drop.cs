using NetMud.Commands.Attributes;
using NetMud.Communication.Lexical;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
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
            List<string> sb = new List<string>();
            IEntity thing = (IEntity)Subject;
            IContains actor = (IContains)Actor;
            IContains place = (IContains)OriginLocation;

            actor.MoveFrom(thing);
            place.MoveInto(thing);

            sb.Add("You drop $S$.");

            Message toActor = new Message(MessagingType.Visible, new SensoryEvent() { Strength = 1 })
            {
                Override = sb
            };

            Message toOrigin = new Message(MessagingType.Visible, new SensoryEvent() { Strength = 30 })
            {
                Override = new string[] { "$A$ drops $S$." }
            };

            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, thing, null, OriginLocation.CurrentRoom, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: drop &lt;object&gt;"
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
                return string.Format("Drop moves an object from your inventory to the room you are currently in.");
            }
            set { }
        }
    }
}
