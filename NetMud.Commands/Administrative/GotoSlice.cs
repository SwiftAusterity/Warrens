using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandKeyword("goto", false, false, true)]
    [CommandPermission(StaffRank.Guest)]
    [CommandParameter(CommandUsage.Subject, typeof(ulong), CacheReferenceType.String, false)] //for names
    [CommandRange(CommandRangeType.Global, 0)]
    public class GotoSlice : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public GotoSlice()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            ulong moveTo = (ulong)Subject;

            var newPos = (IGlobalPosition)Actor.CurrentLocation.Clone();
            newPos.CurrentSection = moveTo;

            Actor.TryMoveTo(newPos);

            var msg = new Message(string.Format("You teleport to {0}.", moveTo));

            msg.ExecuteMessaging(Actor, null, null, null, null, 0);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: goto &lt;section number&gt;"
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
                return string.Format("Goto allows staff members to directly teleport to a section irrespective of its capacity limitations.");
            }
            set { throw new NotImplementedException(); }
        }
    }
}
