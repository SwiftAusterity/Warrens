using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.Comm
{
    [CommandQueueSkip]
    [CommandKeyword("who", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Who : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Who()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        internal override bool ExecutionBody()
        {
            IEnumerable<IPlayer> whoList = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null);

            ILexicalParagraph toActor = new LexicalParagraph(string.Join(",", whoList.Select(who => who.GetDescribableName(Actor))));

            Message messagingObject = new Message(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);

            return true;
        }

        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: who"
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
                return string.Format("Check who is in the world currently.");
            }
            set { }
        }
    }
}
