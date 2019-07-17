using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("say", false, "speak")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.Greedy, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Say : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Say()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IEnumerable<string> toOrigin = new string[] { string.Format("$A$ says '{0}'", Subject) };

            Message messagingObject = new Message(string.Format("You say '{0}'", Subject))
            {
                ToOrigin = toOrigin
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null, 1000);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: say &lt;text&gt;",
                "speak &lt;text&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Say communicates in whatever your current language is to the immediate surroundings. Character with very good hearing may be able to hear from further away.");
            }
            set { }
        }
    }
}
