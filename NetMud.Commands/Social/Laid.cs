using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.Commands.Social
{
    [CommandKeyword("laid", false, new string[] { "fucked", "fapfapfap" }, false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Laid : CommandPartial
    {
        /// <summary>
        /// The delay this invokes when executing. Initially is "begun" and actually takes affect at the end.
        /// </summary>
        public override int ExecutionDelay => 10;

        /// <summary>
        /// The delay this invokes after being executed
        /// </summary>
        public override int CooldownDelay => 20;

        /// <summary>
        /// A message to send the user when the command starts up
        /// </summary>
        public override string StartupMessage => "You begin to fap.";

        /// <summary>
        /// A message to send the user when cooldown finishes
        /// </summary>
        public override string CooldownMessage => "You finish fapping.";

        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Laid()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        internal override bool ExecutionBody()
        {
            Message messagingObject = new Message("You get laid, fucked, fapfapfap.");

            messagingObject.ExecuteMessaging(Actor, null, null, null, null, 0);

            return true;
        }

        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: laid"
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
                return string.Format("Get laid, yo.");
            }
            set { }
        }
    }
}
