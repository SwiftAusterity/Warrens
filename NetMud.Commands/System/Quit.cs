using NetMud.DataStructure.Behaviors.Rendering;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

using NetMud.Utility;
using NetMud.Data.Game;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataAccess.FileSystem;

namespace NutMud.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToLook
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
            var sb = new List<string>();

            var player = (Player)Actor;

            sb.Add("You exit this reality.");

            var messagingObject = new MessageCluster(sb, new string[] { }, new string[] { }, new string[] { "$A$ exits this reality." }, new string[] { });

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null);

            var playerDataWrapper = new PlayerData();

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
            var sb = new List<string>();

            sb.Add("Valid Syntax: quit");
            sb.Add("exit".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Quit/Exit removes your character from the live game allowing you to leave safely or switch characters.");
            }
            set { }
        }
    }
}
