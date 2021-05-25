using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.NPC;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("buy", false, "purchase")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(INonPlayerCharacter), CacheReferenceType.Direction, false)]
    [CommandParameter(CommandUsage.Target, typeof(IInanimate), CacheReferenceType.MerchantStock, false, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Buy : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Buy()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            INonPlayerCharacter merchant = (INonPlayerCharacter)Subject;

            if (merchant == null || !merchant.DoISellThings())
            {
                RenderError("There is no merchant that sells items in that direction.");
                return false;
            }

            IInanimate thing = (IInanimate)Target;

            if (Target == null)
            {
                RenderError("The merchant does not sell that item.");
                return false;
            }

            int price = merchant.PriceCheck(thing, true);

            if (price <= 0)
            {
                RenderError("The merchant will not sell that item.");
                return false;
            }

            string errorMessage = merchant.MakeSale((IMobile)Actor, thing, price);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                RenderError(errorMessage);
            }

            ILexicalParagraph toActor = new LexicalParagraph(string.Format("You purchase a $T$ from $S$ for {0}blz.", price));

            ILexicalParagraph toArea = new LexicalParagraph("$A$ makes a purchase from $S$.");

            //TODO: language outputs
            Message messagingObject = new(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, merchant, thing, OriginLocation.CurrentZone, null);

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
                "Valid Syntax: buy &lt;direction&gt; &lt;item&gt;"
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
                return string.Format("Purchases an item from the merchant.");
            }
            set { }
        }
    }
}
