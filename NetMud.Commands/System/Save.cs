using NutMud.Commands.Attributes;
using System.Collections.Generic;
using NetMud.Data.Game;
using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Data.System;

namespace NutMud.Commands.System
{
    /// <summary>
    /// Invokes the current container's RenderToLook
    /// </summary>
    [CommandKeyword("save", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Save : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Save()
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

            sb.Add("You save your life.");

            var toActor = new Message(MessagingType.Visible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            var messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentLocation, null);

            var playerDataWrapper = new PlayerData();

            //Save the player out
            playerDataWrapper.WriteOnePlayer(player);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: save"
            };

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Save writes your character to the backup set. This also happens automatically behind the scenes quite often.");
            }
            set {  }
        }
    }
}
