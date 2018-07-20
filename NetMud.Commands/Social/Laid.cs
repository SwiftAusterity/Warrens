using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataStructure.SupportingClasses;
using NutMud.Commands.Attributes;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Commands.Social
{
    [CommandKeyword("laid", false, false, true)]
    [CommandKeyword("fucked", false, false, true)]
    [CommandKeyword("fapfapfap", false, false, true)]
    [CommandPermission(StaffRank.Player)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Laid : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Laid()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        public override void Execute()
        {
            var returnStrings = new List<string>();
            var sb = new StringBuilder();

            returnStrings.Add("You get laid, fucked, fapfapfap.");

            var toActor = new Message(MessagingType.Visible, new Occurrence() { Strength = 1 })
            {
                Override = returnStrings
            };

            var messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>
            {
                "Valid Syntax: laid"
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
                return string.Format("Get laid, yo.");
            }
            set { }
        }
    }
}
