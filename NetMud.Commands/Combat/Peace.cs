using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    [CommandQueueSkip]
    [CommandKeyword("peace", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Peace : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Peace()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IEnumerable<string> toOrigin = new string[] { string.Format("$A$ stops fighting.") };

            var msg = new Message("You stop fighting.")
            {
                ToOrigin = toOrigin
            };

            var player = (IPlayer)Actor;

            if (player.IsFighting())
            {
                player.StopFighting();
            }
            else
            {
                msg.ToActor = new string[] { string.Format("You weren't fighting anyone.") };
                msg.ToOrigin = new string[0];
            }

            msg.ExecuteMessaging(Actor, null, null, Actor.CurrentLocation, null, 3);

            return true;
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax: peace")
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
                return @"Peace will cancel all fighting intent. It wont stop someone else from attacking you, though.";
            }
            set { }
        }
    }
}
