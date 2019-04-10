using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandKeyword("gotozone", false, false, true)]
    [CommandPermission(StaffRank.Guest)]
    [CommandParameter(CommandUsage.Subject, typeof(IZone), CacheReferenceType.Entity, false)] //for names
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
