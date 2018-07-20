using System.Collections.Generic;
using NutMud.Commands.Attributes;
using NetMud.DataStructure.Base.Place;
using NetMud.Utility;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Commands.Attributes;
using NetMud.DataStructure.SupportingClasses;
using NutMud.Commands.Rendering;

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
    public class UseExits : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public UseExits()
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

            targetPath.Origin.MoveFrom((IMobile)Actor);
            targetPath.Destination.MoveInto((IMobile)Actor);

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
            var dirList = new List<string>() {
                "east", "north", "northeast", "northwest", "south", "southeast", "southwest", "west", "up", "down",
                "upeast", "upnorth", "upnortheast", "upnorthwest", "upsouth", "upsoutheast", "upsouthwest", "upwest",
                "downeast", "downnorth", "downnortheast", "downnorthwest", "downsouth", "downsoutheast", "downwest",
                "enter &lt;exit name&gt;"
            };

            var sb = new List<string>
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
                return string.Format("These are all directions, need better help text for movements.");
            }
            set { }
        }
    }
}
