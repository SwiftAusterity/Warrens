using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("train", false, "learn")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ITile), CacheReferenceType.Direction, false)]
    [CommandParameter(CommandUsage.Target, typeof(string), CacheReferenceType.String, "(/?|list)", false)]
    [CommandParameter(CommandUsage.Target, typeof(IQuality), CacheReferenceType.TrainerKnowledge, false, true)]
    [CommandParameter(CommandUsage.Target, typeof(IUse), CacheReferenceType.TrainerKnowledge, false, true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Train : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Train()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            ITile tile = (ITile)Subject;

            if (!(tile.TopContents() is INonPlayerCharacter trainer) || (!trainer.DoITeachThings()))
            {
                RenderError("There is no trainer that teaches in that direction.");
                return;
            }

            //Do the list
            if (Target.GetType() == typeof(string))
            {
                var listings = trainer.RenderInstructionList(Actor);

                MessageCluster listingMessage = new MessageCluster(new Message(listings));

                listingMessage.ExecuteMessaging(Actor, null, null, null, null);
            }

            int price = -1;

            string errorMessage = "The trainer can not train that.";

            //We have an ability
            if (Target is IUse ability)
            {
                errorMessage = trainer.Instruct((IMobile)Actor, ability.Name, price);
            }
            else if (Target is IQuality proficency)
            {
                var profLevel = Actor.GetQuality(proficency.Name);

                if (profLevel >= 0)
                {
                    errorMessage = trainer.Instruct((IMobile)Actor, proficency.Name, profLevel + 1, price);
                }
            }

            if (!string.IsNullOrWhiteSpace(errorMessage))
                RenderError(errorMessage);

            Message toArea = new Message("$A$ trains with $S$.");

            //TODO: language outputs
            MessageCluster messagingObject = new MessageCluster(new Message(string.Format("You learn from $S$ for {0}blz.", price)))
            {
                ToOrigin = new List<IMessage> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, trainer, null, OriginLocation.CurrentZone, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: train &lt;direction&gt; &lt;ability|proficency&gt;",
                "train &lt;?|list&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Learn an ability or train profcencies.");
            }
            set { }
        }
    }
}
