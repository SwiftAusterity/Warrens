using NetMud.CentralControl;
using NetMud.Combat;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    [CommandKeyword("stop", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Stop : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Stop()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
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
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax: stop")
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
                return @"Stop will cancel all fighting intent. It wont stop someone else from attacking you, though.";
            }
            set { }
        }
    }
}
