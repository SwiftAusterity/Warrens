using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    [CommandKeyword("shadowbox", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Shadowbox : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Shadowbox()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IEnumerable<string> toOrigin = new string[] { string.Format("$A$ starts to fight with himself.") };

            var msg = new Message("You begin to shadowbox.")
            {
                ToOrigin = toOrigin
            };

            var player = (IPlayer)Actor;

            player.StartFighting(null);

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
                string.Format("Valid Syntax: shadowbox")
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
                return @"Shadowbox is a way to fight yourself. You can't use shadowbox if you're already in combat and someone attacking you will cancel it.";
            }
            set { }
        }
    }
}
