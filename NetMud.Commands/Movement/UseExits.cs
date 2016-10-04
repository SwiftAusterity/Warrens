using System.Collections.Generic;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using NetMud.DataStructure.Base.Place;
using NetMud.Utility;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Commands.Attributes;

namespace NetMud.Commands.Movement
{
    /// <summary>
    /// Handles mobile movement commands. All cardinal directions plus "enter <door>" type pathways
    /// </summary>
    [CommandSuppressName]
    [CommandKeyword("north", true)]
    [CommandKeyword("northeast", true)]
    [CommandKeyword("east", true)]
    [CommandKeyword("southeast", true)]
    [CommandKeyword("south", true)]
    [CommandKeyword("southwest", true)]
    [CommandKeyword("west", true)]
    [CommandKeyword("northwest", true)]
    [CommandKeyword("northwest", true)]
    [CommandKeyword("enter", false)]
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

            targetPath.FromLocation.MoveFrom((IMobile)Actor);
            targetPath.ToLocation.MoveInto((IMobile)Actor);

            targetPath.Enter.ExecuteMessaging(Actor, targetPath, null, targetPath.FromLocation, targetPath.ToLocation);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Valid Syntax:"));
            sb.Add("north".PadWithString(14, "&nbsp;", true));
            sb.Add("northeast".PadWithString(14, "&nbsp;", true));
            sb.Add("east".PadWithString(14, "&nbsp;", true));
            sb.Add("southeast".PadWithString(14, "&nbsp;", true));
            sb.Add("south".PadWithString(14, "&nbsp;", true));
            sb.Add("southwest".PadWithString(14, "&nbsp;", true));
            sb.Add("west".PadWithString(14, "&nbsp;", true));
            sb.Add("northwest".PadWithString(14, "&nbsp;", true));
            sb.Add("enter &lt;exit name&gt;".PadWithString(14, "&nbsp;", true));

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
