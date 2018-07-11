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
    [CommandKeyword("east", true)]
    [CommandKeyword("north", true)]
    [CommandKeyword("northeast", true)]
    [CommandKeyword("northwest", true)]
    [CommandKeyword("south", true)]
    [CommandKeyword("southwest", true)]
    [CommandKeyword("southeast", true)]
    [CommandKeyword("west", true)]
    [CommandKeyword("up", true)]
    [CommandKeyword("down", true)]
    [CommandKeyword("upnorth", true)]
    [CommandKeyword("upnortheast", true)]
    [CommandKeyword("upnorthwest", true)]
    [CommandKeyword("upsouth", true)]
    [CommandKeyword("upsouthwest", true)]
    [CommandKeyword("upsoutheast", true)]
    [CommandKeyword("upwest", true)]
    [CommandKeyword("downnorth", true)]
    [CommandKeyword("downnortheast", true)]
    [CommandKeyword("downnorthwest", true)]
    [CommandKeyword("downsouth", true)]
    [CommandKeyword("downsouthwest", true)]
    [CommandKeyword("downsoutheast", true)]
    [CommandKeyword("downwest", true)]
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
