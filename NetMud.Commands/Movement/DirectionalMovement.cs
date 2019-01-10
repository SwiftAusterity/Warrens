using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Room;
using NetMud.Utility;
using NutMud.Commands.Rendering;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Handles mobile movement commands. All cardinal directions plus "enter <door>" type pathways
    /// </summary>
    [CommandSuppressName]
    [CommandKeyword("enter", false)]
    [CommandKeyword("east", true, false, true)]
    [CommandKeyword("north", true, false, true)]
    [CommandKeyword("northeast", true, false, true)]
    [CommandKeyword("northwest", true, false, true)]
    [CommandKeyword("south", true, false, true)]
    [CommandKeyword("southwest", true, false, true)]
    [CommandKeyword("southeast", true, false, true)]
    [CommandKeyword("west", true, false, true)]
    [CommandKeyword("up", true, false, true)]
    [CommandKeyword("down", true, false, true)]
    [CommandKeyword("upnorth", true, false, true)]
    [CommandKeyword("upnortheast", true, false, true)]
    [CommandKeyword("upnorthwest", true, false, true)]
    [CommandKeyword("upsouth", true, false, true)]
    [CommandKeyword("upsouthwest", true, false, true)]
    [CommandKeyword("upsoutheast", true, false, true)]
    [CommandKeyword("upwest", true, false, true)]
    [CommandKeyword("downnorth", true, false, true)]
    [CommandKeyword("downnortheast", true, false, true)]
    [CommandKeyword("downnorthwest", true, false, true)]
    [CommandKeyword("downsouth", true, false, true)]
    [CommandKeyword("downsouthwest", true, false, true)]
    [CommandKeyword("downsoutheast", true, false, true)]
    [CommandKeyword("downwest", true, false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IPathway), new CacheReferenceType[] { CacheReferenceType.Entity }, "[a-zA-z]+", true)]
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
            var sb = new List<string>();
            IPathway targetPath = (IPathway)Subject;

            Actor.TryMoveTo(targetPath.Destination.CurrentLocation);

            targetPath.Enter.ExecuteMessaging(Actor, targetPath, null, targetPath.Origin, targetPath.Destination);

            //Render the next room to them
            var lookCommand = new Look() { Actor = Actor, Subject = null, OriginLocation = Actor.CurrentLocation };

            lookCommand.Execute();
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> dirList = new List<string>() {
                "east", "north", "northeast", "northwest", "south", "southeast", "southwest", "west", "Up", "Down"
                , "Upnorth", "Upnortheast", "Upnorthwest", "Upsouth", "Upsouthwest", "Upsoutheast", "Upwest"
                , "Downnorth", "Downnortheast", "Downnorthwest", "Downsouth", "Downsouthwest", "Downsoutheast", "Downwest"
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

Movement is accomplished via the 8 cardinal directions + elevation:

- North
- East
- South
- West
- Northwest
- Northeast
- Southwest
- Southeast
- Up
- Down
- Upnorth
- Upnortheast
- Upnorthwest
- Upsouth
- Upsouthwest
- Upsoutheast
- Upwest
- Downnorth
- Downnortheast
- Downnorthwest
- Downsouth
- Downsouthwest
- Downsoutheast
- Downwest

The web client binds the arrow keys and the numpad keys to their appropriate direction. Each direction also has a shortened alias of the first (or relevant 2) letter of the direction.
";
            }
            set { }
        }
    }
}
