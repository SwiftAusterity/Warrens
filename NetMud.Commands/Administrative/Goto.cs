using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Data.Game;

namespace NutMud.Commands.Administrative
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("goto", false)]
    [CommandPermission(StaffRank.Guest)]
    [CommandParameter(CommandUsage.Subject, typeof(Room), new CacheReferenceType[] { CacheReferenceType.Entity }, true)]
    [CommandRange(CommandRangeType.Global, 0)]
    public class Goto : ICommand, IHelpful
    {
        /// <summary>
        /// The entity invoking the command
        /// </summary>
        public IActor Actor { get; set; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        public object Subject { get; set; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        public object Supporting { get; set; }

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        public ILocation OriginLocation { get; set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; set; }

        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Goto()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public void Execute()
        {
            var moveTo = (ILocation)Subject;
            var sb = new List<string>();

            sb.Add("You teleport.");

            var messagingObject = new MessageCluster(RenderUtility.EncapsulateOutput(sb), string.Empty, string.Empty, "$A$ disappears in a puff of smoke.", "$A$ appears out of nowhere.");

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null);

            moveTo.MoveInto<Player>((Player)Actor);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: goto &lt;room name&gt;");

            return sb;
        }

        /// <summary>
        /// Renders the help text
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Goto allows staff members to directly teleport to a room irrespective of its capacity limitations."));

            return sb;
        }
    }
}
