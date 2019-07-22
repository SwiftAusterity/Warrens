using NetMud.Commands.Attributes;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Room;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Handles mobile movement commands. All cardinal directions plus "enter <door>" type pathways
    /// </summary>
    [CommandSuppressName]
    [CommandKeyword("enter", false)]
    [CommandKeyword("east", true, "e", false, true)]
    [CommandKeyword("north", true, "n", false, true)]
    [CommandKeyword("northeast", true, "ne", false, true)]
    [CommandKeyword("northwest", true, "nw", false, true)]
    [CommandKeyword("south", true, "s", false, true)]
    [CommandKeyword("southwest", true, "sw", false, true)]
    [CommandKeyword("southeast", true, "se", false, true)]
    [CommandKeyword("west", true, "w", false, true)]
    [CommandKeyword("up", true, "u", false, true)]
    [CommandKeyword("down", true, "d", false, true)]
    [CommandKeyword("upnorth", true, "un", false, true)]
    [CommandKeyword("upnortheast", true, "une", false, true)]
    [CommandKeyword("upnorthwest", true, "unw", false, true)]
    [CommandKeyword("upsouth", true, "us", false, true)]
    [CommandKeyword("upsouthwest", true, "usw", false, true)]
    [CommandKeyword("upsoutheast", true, "use", false, true)]
    [CommandKeyword("upwest", true, "uw", false, true)]
    [CommandKeyword("downnorth", true, "dn", false, true)]
    [CommandKeyword("downnortheast", true, "dne", false, true)]
    [CommandKeyword("downnorthwest", true, "dnw", false, true)]
    [CommandKeyword("downsouth", true, "ds", false, true)]
    [CommandKeyword("downsouthwest", true, "dsw", false, true)]
    [CommandKeyword("downsoutheast", true, "dse", false, true)]
    [CommandKeyword("downwest", true, "dw", false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IPathway), new CacheReferenceType[] { CacheReferenceType.Pathway }, "[a-zA-z]+", true)]
    [CommandParameter(CommandUsage.Subject, typeof(IPathway), new CacheReferenceType[] { CacheReferenceType.Direction }, "[a-zA-z]+", true)]
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
        internal override bool ExecutionBody()
        {
            IPathway targetPath = (IPathway)Subject;

            Actor.TryMoveTo(targetPath.Destination.GetContainerAsLocation());

            targetPath.Enter.ExecuteMessaging(Actor, targetPath, null, targetPath.Origin, targetPath.Destination);

            Actor.WriteTo(null);

            return true;
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
