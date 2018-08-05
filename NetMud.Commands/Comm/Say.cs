using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.Lexical;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
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
            var sb = new List<string>
            {
                string.Format("You say '{0}'", Subject)
            };

            var toActor = new Message(MessagingType.Audible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            var areaString = new string[] { string.Format("$A$ says '{0}'", Subject) };

            var toArea = new Message(MessagingType.Audible, new Occurrence() { Strength = 30 })
            {
                Override = areaString
            };

            //TODO: language outputs
            var messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toArea }
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
                "Valid Syntax: say &lt;text&gt;",
                "speak &lt;text&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Say communicates in whatever your current language is to the immediate surroundings. Characters with very good hearing may be able to hear from further away.");
            }
            set { }
        }
    }
}
