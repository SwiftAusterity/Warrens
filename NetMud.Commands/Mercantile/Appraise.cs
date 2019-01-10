using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("appraise", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IInanimate), CacheReferenceType.Inventory, false)]
    [CommandParameter(CommandUsage.Target, typeof(INonPlayerCharacter), CacheReferenceType.Direction, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Appraise : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Appraise()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            IInanimate thing = (IInanimate)Subject;

            if (Target == null)
            {
                RenderError("There is no merchant in that direction.");
                return;
            }

            INonPlayerCharacter merchant = (INonPlayerCharacter)Target;

            if (merchant == null || (!merchant.DoIBuyThings()))
            {
                RenderError("There is no merchant that buys items in that direction.");
                return;
            }

            string errorMessage = string.Empty;

            int price = merchant.HaggleCheck(thing);

            if(price <= 0)
            {
                RenderError("The merchant will not buy that item.");
                return;
            }

            List<string> sb = new List<string>
            {
                string.Format("The merchant appraises your {0} at {1}blz.", thing.GetDescribableName(Actor), price)
            };

            Message toActor = new Message()
            {
                Override = sb
            };

            string[] areaString = new string[] { "$T$ looks very closely at $A$'s $S$." };

            Message toArea = new Message()
            {
                Override = areaString
            };

            //TODO: language outputs
            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, thing, merchant, OriginLocation.CurrentZone, null);

        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: appraise &lt;direction&gt; &lt;item&gt;"
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
                return string.Format("Has the merchant appraise an item in your inventory for purchase.");
            }
            set { }
        }
    }
}
