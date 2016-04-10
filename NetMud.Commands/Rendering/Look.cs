using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;

namespace NutMud.Commands.Rendering
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("look", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(ILookable), new CacheReferenceType[] { CacheReferenceType.Entity }, true )]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Look : CommandPartial, IHelpful
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Look()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();

            //Just do a look on the room
            if (Subject == null)
                sb.AddRange(OriginLocation.RenderToLook(Actor));
            else
            {
                var lookTarget = (ILookable)Subject;
                sb.AddRange(lookTarget.RenderToLook(Actor));
            }

            var messagingObject = new MessageCluster(sb, new string[] { "$A$ looks at you." }, new string[] { }, new string[] { "$A$ looks around the room." }, new string[] { });

            messagingObject.ExecuteMessaging(Actor, (IEntity)Subject, null, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: look");
            sb.Add("look &lt;target&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// Renders the help text
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Look provides useful information about the location you are in or a target object or mobile."));

            return sb;
        }
    }
}
