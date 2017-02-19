using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System;
using System.Collections.Generic;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("say", false)]
    [CommandKeyword("speak", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Text }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Say : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Say()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var sb = new List<string>();

            sb.Add(String.Format("You say '{0}'", Subject));

            var toActor = new Message(MessagingType.Audible, 1);
            toActor.Override = sb;

            var areaString = new string[] { String.Format("$A$ says '{0}'", Subject) };

            var toArea = new Message(MessagingType.Audible, 30);
            toArea.Override = areaString;

            //TODO: language outputs
            var messagingObject = new MessageCluster(toActor);
            messagingObject.ToOrigin = new List<IMessage> { toArea };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add("Valid Syntax: say &lt;text&gt;");
            sb.Add("speak &lt;text&gt;".PadWithString(14, "&nbsp;", true));

            return sb;
        }

        /// <summary>
        /// The custom body of help text
        /// </summary>
        public override string HelpText
        {
            get
            {
                return string.Format("Say communicates in whatever your current language is to the immediate surroundings. Characters with very good hearing may be able to hear from further away.");
            }
            set { }
        }
    }
}
