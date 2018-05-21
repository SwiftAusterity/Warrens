using System.Collections.Generic;
using NutMud.Commands.Attributes;
using NetMud.DataStructure.Base.Place;
using NetMud.Utility;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Commands.Attributes;
using NetMud.DataStructure.SupportingClasses;

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
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                string.Format("Valid Syntax:"),
                "east".PadWithString(14, "&nbsp;", true),
                "north".PadWithString(14, "&nbsp;", true),
                "northeast".PadWithString(14, "&nbsp;", true),
                "northwest".PadWithString(14, "&nbsp;", true),
                "south".PadWithString(14, "&nbsp;", true),
                "southeast".PadWithString(14, "&nbsp;", true),
                "southwest".PadWithString(14, "&nbsp;", true),
                "west".PadWithString(14, "&nbsp;", true),
                "up".PadWithString(14, "&nbsp;", true),
                "down".PadWithString(14, "&nbsp;", true),
                "upeast".PadWithString(14, "&nbsp;", true),
                "upnorth".PadWithString(14, "&nbsp;", true),
                "upnortheast".PadWithString(14, "&nbsp;", true),
                "upnorthwest".PadWithString(14, "&nbsp;", true),
                "upsouth".PadWithString(14, "&nbsp;", true),
                "upsoutheast".PadWithString(14, "&nbsp;", true),
                "upsouthwest".PadWithString(14, "&nbsp;", true),
                "upwest".PadWithString(14, "&nbsp;", true),
                "downeast".PadWithString(14, "&nbsp;", true),
                "downnorth".PadWithString(14, "&nbsp;", true),
                "downnortheast".PadWithString(14, "&nbsp;", true),
                "downnorthwest".PadWithString(14, "&nbsp;", true),
                "downsouth".PadWithString(14, "&nbsp;", true),
                "downsoutheast".PadWithString(14, "&nbsp;", true),
                "downsouthwest".PadWithString(14, "&nbsp;", true),
                "downwest".PadWithString(14, "&nbsp;", true),

                "enter &lt;exit name&gt;".PadWithString(14, "&nbsp;", true)
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("These are all directions, need better help text for movements.");
            }
            set { }
        }
    }
}
