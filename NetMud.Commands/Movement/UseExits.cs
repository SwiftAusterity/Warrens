using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using NetMud.DataStructure.Base.Place;
using NetMud.Utility;
using NetMud.DataStructure.Base.Supporting;

namespace NetMud.Commands.Movement
{
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
    [CommandParameter(CommandUsage.Subject, typeof(IPath), new CacheReferenceType[] { CacheReferenceType.Entity }, "[a-zA-z]+", true)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class UseExits : ICommand, IHelpful
    {
        public ILocation OriginLocation { get; set; }
        public IEnumerable<ILocation> Surroundings { get; set; }

        public object Subject { get; set; }
        public object Supporting { get; set; }
        public object Target { get; set; }
        public IActor Actor { get; set; }

        public void Execute()
        {
            var sb = new List<string>();
            IPath targetPath=(IPath)Subject;

            targetPath.FromLocation.MoveFrom((IMobile)Actor);
            targetPath.ToLocation.MoveInto((IMobile)Actor);

            targetPath.Enter.ExecuteMessaging(Actor, targetPath, null, targetPath.FromLocation, targetPath.ToLocation);
        }

        public IEnumerable<string> RenderSyntaxHelp()
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

        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("These are all directions, need better help text for movements."));

            return sb;
        }
    }
}
