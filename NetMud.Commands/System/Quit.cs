using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToVisible
    /// </summary>
    [CommandKeyword("quit", false)]
    [CommandKeyword("exit", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Quit : CommandPartial
    {
         /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Quit()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            List<string> sb = new List<string>();

            IPlayer player = (IPlayer)Actor;
            ILexicalParagraph toActor = new LexicalParagraph("You exit this reality.");

            ILexicalParagraph toOrigin = new LexicalParagraph("$A$ exits this reality.");


            Message messagingObject = new Message(toActor)
            {
                ToOrigin = new List<ILexicalParagraph> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);

            PlayerData playerDataWrapper = new PlayerData();

            //Save the player out
            playerDataWrapper.WriteOnePlayer(player);
            player.CloseConnection();
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                "Valid Syntax: quit",
                "exit".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Quit/Exit removes your character from the live game allowing you to leave safely or switch characters.");
            }
            set { }
        }
    }
}
