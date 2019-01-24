using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.NPC;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("list", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.String, "(buy|sell)$", true)]
    [CommandParameter(CommandUsage.Target, typeof(INonPlayerCharacter), CacheReferenceType.Direction, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class List : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public List()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();
            bool wantsSellSheet = Subject == null || string.IsNullOrWhiteSpace(Subject.ToString()) || Subject.ToString() != "buy";

            if (Target == null)
            {
                RenderError("There is no merchant in that direction.");
                return;
            }

            INonPlayerCharacter merchant = (INonPlayerCharacter)Target;

            if (merchant == null || (!merchant.DoIBuyThings() && !merchant.DoISellThings()))
            {
                RenderError("There is no merchant in that direction.");
                return;
            }

            string errorMessage = string.Empty;
            if (wantsSellSheet)
            {
                errorMessage = merchant.RenderInventory(Actor);
            }
            else
            {
                errorMessage = merchant.RenderPurchaseSheet(Actor);
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
                RenderError(errorMessage);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: list &lt;direction&gt;",
                "list &lt;direction&gt; buy|sell".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Has the merchant list their sellable inventory or their buy sheet.");
            }
            set { }
        }
    }
}
