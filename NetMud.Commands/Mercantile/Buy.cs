using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("buy", false, "purchase")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ITile), CacheReferenceType.Direction, false)]
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
        public override void Execute()
        {
            ITile tile = (ITile)Subject;

            if (!(tile.TopContents() is INonPlayerCharacter merchant) || (!merchant.DoISellThings()))
            {
                RenderError("There is no merchant that sells items in that direction.");
                return;
            }

            IInanimate thing = (IInanimate)Target;

            if (Target == null)
            {
                RenderError("The merchant does not sell that item.");
                return;
            }

            int price = merchant.PriceCheck(thing, true);

            if (price <= 0)
            {
                RenderError("The merchant will not sell that item.");
                return;
            }

            string errorMessage = merchant.MakeSale((IMobile)Actor, thing, price);

            if (!string.IsNullOrWhiteSpace(errorMessage))
                RenderError(errorMessage);

            List<string> sb = new List<string>
            {
                string.Format("You purchase a $T$ from $S$ for {0}blz.", price)
            };

            Message toActor = new Message()
            {
                Body = sb
            };

            string[] areaString = new string[] { "$A$ makes a purchase from $S$." };

            Message toArea = new Message()
            {
                Body = areaString
            };

            //TODO: language outputs
            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toArea }
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
