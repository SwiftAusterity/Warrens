using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("gotochar", false, false, true)]
    [CommandPermission(StaffRank.Guest)]
    [CommandParameter(CommandUsage.Subject, new Type[] { typeof(IPlayer), typeof(INonPlayerCharacter) }, CacheReferenceType.Entity, true)]
    [CommandRange(CommandRangeType.Global, 0)]
    public class GotoChar : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public GotoChar()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var moveToPerson = (IEntity)Subject;

            if (moveToPerson.CurrentLocation == null)
                throw new Exception("Invalid goto target.");

            var moveTo = (IGlobalPosition)moveToPerson.CurrentLocation.Clone();

            List<string> sb = new List<string>
            {
                "You teleport."
            };

            Message toActor = new Message()
            {
                Override = sb
            };

            Message toOrigin = new Message()
            {
                Override = new string[] { "$A$ disappears in a puff of smoke." }
            };

            Message toDest = new Message()
            {
                Override = new string[] { "$A$ appears out of nowhere." }
            };

            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin },
                ToDestination = new List<IMessage> { toDest }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

            Actor.TryTeleport(moveTo);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
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
