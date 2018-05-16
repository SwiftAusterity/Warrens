using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

namespace NetMud.Commands.Rendering
{
    [CommandKeyword("inventory", false)]
    [CommandKeyword("inv", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Inventory : CommandPartial
    {        
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Inventory()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();
            var chr = (IMobile)Actor;

            sb.Add("You look through your belongings.");

            foreach (var thing in chr.Inventory.EntitiesContained())
                sb.AddRange(thing.RenderToLook(chr));

            var toActor = new Message(MessagingType.Visible, 1)
            {
                Override = sb
            };

            var toOrigin = new Message(MessagingType.Visible, 30)
            {
                Override = new string[] { "$A$ sifts through $G$ belongings." }
            };

            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toOrigin }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: inventory",
                "inv".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Inventory lists out all inanimates currently on your person. It is an undetectable action unless a viewer has high perception.");
            }
            set { }
        }

    }
}
