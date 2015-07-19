using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;

using NetMud.Utility;

namespace NutMud.Commands.System
{
    //Really help can be invoked on anything that is helpful, even itself
    [CommandKeyword("look", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ILookable), new CacheReferenceType[] { CacheReferenceType.Entity }, true )]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Look : ICommand, IHelpful
    {
        public IActor Actor { get; set; }
        public object Subject { get; set; }
        public object Target { get; set; }
        public object Supporting { get; set; }
        public ILocation OriginLocation { get; set; }
        public IEnumerable<ILocation> Surroundings { get; set; }

        public Look()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        public IEnumerable<string> Execute()
        {
            var sb = new List<string>();

            //Just do a look on the room
            if (Subject == null)
                sb.AddRange(OriginLocation.RenderToLook());
            else
            {
                var lookTarget = (ILookable)Subject;
                sb.AddRange(lookTarget.RenderToLook());
            }

            return sb;
        }

        public IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: look");
            sb.Add("look &lt;target&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text for the help command itself
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(String.Format("Look provides useful information about the location you are in or a target object or mobile."));

            return sb;
        }
    }
}
