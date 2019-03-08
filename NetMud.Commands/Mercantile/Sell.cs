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
    [CommandKeyword("sell", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(INonPlayerCharacter), CacheReferenceType.Direction, false)]
    [CommandParameter(CommandUsage.Target, typeof(IInanimate), CacheReferenceType.Inventory, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Sell : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Sell()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            INonPlayerCharacter merchant = (INonPlayerCharacter)Subject;

            if (merchant == null || !merchant.DoIBuyThings())
            {
                RenderError("There is no merchant that buys items in that direction.");
                return;
            }

            IInanimate thing = (IInanimate)Target;

            if (Target == null)
            {
                RenderError("That item does not exist.");
                return;
            }

            int price = merchant.HaggleCheck(thing);

            if (price <= 0)
            {
                RenderError("The merchant will not buy that item.");
                return;
            }

            string errorMessage = merchant.MakePurchase((IMobile)Actor, thing, price);

            if (!string.IsNullOrWhiteSpace(errorMessage))
            {
                RenderError(errorMessage);
            }

            ILexicalParagraph toActor = new LexicalParagraph(string.Format("You sell a $T$ to $S$ for {0}blz.", price));

            ILexicalParagraph toArea = new LexicalParagraph("$A$ sells an item to $S$.");

            //TODO: language outputs
            Message messagingObject = new Message(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, merchant, thing, OriginLocation.CurrentZone, null);

        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: sell &lt;direction&gt; &lt;item&gt;"
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
                return string.Format("Has the merchant buy an item in your inventory.");
            }
            set { }
        }
    }
}
