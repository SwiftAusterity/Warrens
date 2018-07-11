using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.Data.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("shout", false)]
    [CommandKeyword("yell", false)]
    [CommandKeyword("scream", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), new CacheReferenceType[] { CacheReferenceType.Text }, false)]
    [CommandRange(CommandRangeType.Touch, 0)]
    public class Shout : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Shout()
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
                string.Format("You shout '{0}'", Subject)
            };

            var toActor = new Message(MessagingType.Audible, new Occurrence() { Strength = 1 })
            {
                Override = sb
            };

            var areaString = new string[] { string.Format("$A$ shouts '{0}'", Subject) };

            var toArea = new Message(MessagingType.Audible, new Occurrence() { Strength = 900 })
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
                "Valid Syntax: shout &lt;text&gt;",
                "yell &lt;text&gt;".PadWithString(14, "&nbsp;", true),
                "scream &lt;text&gt;".PadWithString(14, "&nbsp;", true)
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
                return string.Format("Shout communicates in whatever your current language is to the immediate surroundings at an audible strength 30x say/speak. Characters with good hearing may be able to hear from further away.");
            }
            set { }
        }
    }
}
