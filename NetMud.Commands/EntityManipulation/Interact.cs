using NetMud.Commands.Attributes;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("interact", false)]
    [CommandPermission(StaffRank.Player, true)]
    [CommandParameter(CommandUsage.Subject, typeof(ITile), CacheReferenceType.Direction, false)]
    [CommandParameter(CommandUsage.Target, typeof(IInteraction), CacheReferenceType.Interaction, false, true)]
    [CommandParameter(CommandUsage.Supporting, typeof(IInanimate), CacheReferenceType.Inventory, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Interact : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Interact()
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
                RenderError("That is an invalid interaction.");
                return;
            }

            ITile tile = (ITile)Subject;
            IInteraction interaction = (IInteraction)Target;
            IInanimate tool = Supporting as IInanimate;

            string errorMessage = interaction.Invoke(Actor, tile, tool);

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
                "Valid Syntax: Interact &lt;direction&gt; &lt;interaction name&gt;"
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
                return string.Format("Invokes an interaction with the tile or object or npc in a direction.");
            }
            set { }
        }
    }
}
