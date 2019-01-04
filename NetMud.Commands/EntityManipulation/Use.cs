using NetMud.Commands.Attributes;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Tile;
using NetMud.Utility;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("use", false)]
    [CommandPermission(StaffRank.Player, true)]
    [CommandParameter(CommandUsage.Subject, typeof(IInanimate), CacheReferenceType.Inventory, false)]
    [CommandParameter(CommandUsage.Target, typeof(IUse), CacheReferenceType.Use, false, true)]
    [CommandParameter(CommandUsage.Supporting, typeof(ITile), CacheReferenceType.Direction, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Use : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Use()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();

            if (Subject == null || Target == null)
            {
                RenderError("That is an invalid use.");
                return;
            }

            IInanimate useItem = (IInanimate)Subject;
            IUse interaction = (IUse)Target;
            ITile targetTile = Supporting as ITile;
            string errorMessage = string.Empty;

            if (targetTile == null && (interaction.Criteria.Count() > 0 && interaction.Criteria.Any(crit => crit.Target != ActionTarget.Self && crit.Target != ActionTarget.Tool)))
            {
                errorMessage = "You must specify a direction for that use.";
            }
            else
            {
                errorMessage = interaction.Invoke(Actor, targetTile, useItem);
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
                "Valid Syntax: use &lt;item&gt; &lt;interaction name&gt; &lt;direction&gt;",
                "use &lt;item&gt; &lt;interaction name&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Invokes a use of an item in your inventory against a tile or item in a direction next to you.");
            }
            set { }
        }
    }
}
