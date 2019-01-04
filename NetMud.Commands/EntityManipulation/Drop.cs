using NetMud.Cartography;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("drop", false, new string[] { "place", "put" })]
    [CommandPermission(StaffRank.Player, true)]
    [CommandParameter(CommandUsage.Subject, typeof(IInanimate), CacheReferenceType.Inventory, false)]
    [CommandParameter(CommandUsage.Target, typeof(ITile), CacheReferenceType.Direction, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Drop : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Drop()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();
            IInanimate thing = (IInanimate)Subject;
            var actor = (IContains)Actor;

            if (Target == null)
            {
                RenderError("There is nothing in that direction.");
                return;
            }

            ITile tile = (ITile)Target;

            var intruder = tile.TopContents()?.Template<ITemplate>();

            if (intruder != null)
            {
                RenderError("There is something in your way.");
                return;
            }

            string error = thing.TryMoveTo(actor.GetContainerAsLocation());

            if(!string.IsNullOrWhiteSpace(error))
            {
                RenderError(error);
                return;
            }

            sb.Add("You drop $S$.");

            Message toActor = new Message()
            {
                Body = sb
            };

            Message toOrigin = new Message()
            {
                Body = new string[] { "$A$ drops $S$." }
            };

            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, thing, null, OriginLocation.CurrentZone, null);

            //Cause a map delta
            Utilities.SendMapUpdatesToZone(Actor.CurrentLocation.CurrentZone, new HashSet<Coordinate>() { tile.Coordinate });
            Actor.Save();
            tile.ParentLocation.Save();
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: drop &lt;object&gt; &lt;direction&gt;"
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
                return string.Format("Drop moves an object from your inventory to the room you are currently in.");
            }
            set { }
        }
    }
}
