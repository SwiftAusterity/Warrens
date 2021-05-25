using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
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
        internal override bool ExecutionBody()
        {
            IEntity moveToPerson = (IEntity)Subject;

            if (moveToPerson.CurrentLocation == null)
            {
                throw new Exception("Invalid goto target.");
            }

            IGlobalPosition moveTo = (IGlobalPosition)moveToPerson.CurrentLocation.Clone();

            LexicalParagraph toActor = new()
            {
                Override = "You teleport."
            };

            LexicalParagraph toOrigin = new()
            {
                Override = "$A$ disappears in a puff of smoke."
            };

            LexicalParagraph toDest = new()
            {
                Override = "$A$ appears out of nowhere."
            };

            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin },
                ToDestination = new List<ILexicalParagraph> { toDest }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

            Actor.TryTeleport(moveTo);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new()
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
