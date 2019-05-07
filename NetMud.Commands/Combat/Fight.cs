using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Player;
using System.Collections.Generic;

namespace NetMud.Commands.Movement
{
    [CommandKeyword("fight", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IActor), new CacheReferenceType[] { CacheReferenceType.Entity }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Fight : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Fight()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        internal override bool ExecutionBody()
        {
            IEnumerable<string> toOrigin = new string[] { string.Format("$A$ starts to fight with $T$.") };
            IEnumerable<string> toVict = new string[] { string.Format("$A$ ATTACKS YOU.") };

            var msg = new Message("You begin to fight $T$.")
            {
                ToOrigin = toOrigin,
                ToSubject = toVict
            };

            var player = (IPlayer)Actor;
            var victim = (IPlayer)Subject;

            player.StartFighting(victim);

            msg.ExecuteMessaging(player, null, victim, Actor.CurrentLocation, null, 3);

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
                string.Format("Valid Syntax: fight &lt;target&gt;")
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
                return @"Fight allows you to engage in combat with another player. You can't use fight if you're already in combat. You can use PEACE to stop fighting but it wont stop the other guy!";
            }
            set { }
        }
    }
}
