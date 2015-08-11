using NetMud.Commands.Attributes;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("say", false)]
    [CommandKeyword("speak", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Text }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Say : CommandPartial, IHelpful
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
        public override void Execute()
        {
            var sb = new List<string>();

            sb.Add(String.Format("You say '{0}'", Subject));

            //TODO: language outputs
            var messagingObject = new MessageCluster(RenderUtility.EncapsulateOutput(sb), string.Empty, string.Empty, String.Format("$A$ says '{0}'", Subject), string.Empty);

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: say &lt;text&gt;");
            sb.Add("speak &lt;text&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Say communicates in whatever your current language is to the immediate surroundings. Characters with very good hearing may be able to hear from further away."));

            return sb;
        }
    }
}
