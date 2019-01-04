using NetMud.Cartography;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.EntityManipulation
{
    [CommandKeyword("get", false, "take")]
    [CommandPermission(StaffRank.Player, true)]
    [CommandParameter(CommandUsage.Subject, typeof(ITile), CacheReferenceType.Direction, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Get : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Get()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();
            IContains actor = (IContains)Actor;

            string toRoomMessage = "$A$ gets $S$.";

            if (Subject == null)
            {
                RenderError("There is nothing in that direction.");
                return;
            }

            ITile tile = (ITile)Subject;
            IEntity thing = tile.TopContents();

            if (thing == null)
            {
                RenderError("There is nothing there.");
                return;
            }

            string error = thing.TryMoveTo(actor.GetContainerAsLocation());

            if (!string.IsNullOrWhiteSpace(error))
            {
                RenderError(error);
                return;
            }

            sb.Add("You get $S$.");

            Message toActor = new Message()
            {
                Body = sb
            };

            Message toOrigin = new Message()
            {
                Body = new string[] { toRoomMessage }
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
                "Valid Syntax: get &lt;direction&gt;",
                "take &lt;direction&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Takes an object from a container or if unspecified attempts to take it from the room you are in.");
            }
            set { }
        }
    }
}
