using NetMud.Cartography;
using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Handles mobile movement commands. All cardinal directions plus "enter <door>" type pathways
    /// </summary>
    [CommandSuppressName]
    [CommandKeyword("east", false, "e", false)]
    [CommandKeyword("north", false, "n", false)]
    [CommandKeyword("northeast", false, "ne", false)]
    [CommandKeyword("northwest", false, "nw", false)]
    [CommandKeyword("south", false, "s", false)]
    [CommandKeyword("southwest", false, "sw", false)]
    [CommandKeyword("southeast", false, "se", false)]
    [CommandKeyword("west", false, "w", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class DirectionalMovement : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public DirectionalMovement()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();

            var x = Actor.CurrentLocation.CurrentCoordinates.X;
            var y = Actor.CurrentLocation.CurrentCoordinates.Y;

            Coordinate originCoords = new Coordinate(x, y);
            switch (CommandWord)
            {
                case "east":
                    x++;
                    break;
                case "north":
                    y++;
                    break;
                case "northeast":
                    x++;
                    y++;
                    break;
                case "northwest":
                    x--;
                    y++;
                    break;
                case "south":
                    y--;
                    break;
                case "southwest":
                    x--;
                    y--;
                    break;
                case "southeast":
                    x++;
                    y--;
                    break;
                case "west":
                    x--;
                    break;
            }

            if (!x.IsBetweenOrEqual(0, 99) || !y.IsBetweenOrEqual(0, 99))
            {
                RenderError("That is an invalid direction to walk.");
                return;
            }

            IGlobalPosition currentPosition = Actor.CurrentLocation.Clone(new Coordinate(x, y));

            DataStructure.Tile.ITile tile = currentPosition.GetTile();
            if (tile == null || tile.Type == null || !tile.Type.Pathable)
            {
                RenderError("That is an invalid direction to walk.");
                return;
            }

            ITemplate intruder = tile.TopContents()?.Template<ITemplate>();

            if (intruder != null)
            {
                RenderError("There is something in your way.");
                return;
            }

            Actor.TryMoveTo(currentPosition);

            Utilities.SendMapUpdatesToZone(Actor.CurrentLocation.CurrentZone, new HashSet<Coordinate>() { originCoords, currentPosition.CurrentCoordinates });
            Actor.Save();
            currentPosition.CurrentZone.Save();
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> dirList = new List<string>() {
                "east", "north", "northeast", "northwest", "south", "southeast", "southwest", "west",
            };

            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax:"),
                dirList.CommaList(RenderUtility.SplitListType.AllComma)
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
                return @"
### Movement

Movement is accomplished via the 8 cardinal directions:

- North
- East
- South
- West
- Northwest
- Northeast
- Southwest
- Southeast

The web client binds the arrow keys and the numpad keys to their appropriate direction. Each direction also has a shortened alias of the first (or relevant 2) letter of the direction.
";
            }
            set { }
        }
    }
}
