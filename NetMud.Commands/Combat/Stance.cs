using NetMud.CentralControl;
using NetMud.Combat;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    [CommandKeyword("stance", false)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.Greedy, false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Stance : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Stance()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            var newStance = Subject.ToString();
            var player = (IPlayer)Actor;

            player.Stance = newStance;

            var msg = new Message(string.Format("You change your stance to {0}.", newStance));

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
                string.Format("Valid Syntax: stance &lt;new stance&gt;")
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
                return @"Stance allows you to change your fighting stance. It is free-text and is tied to what stance you set for your Fighting Art Combos.";
            }
            set { }
        }
    }
}
