﻿using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.System;
using NetMud.Utility;
using System.Collections.Generic;

namespace NetMud.Commands.Comm
{
    [CommandKeyword("shout", false, new string[] { "yell", "scream", "global" })]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(string), CacheReferenceType.Greedy, false)]
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
            List<string> sb = new List<string>
            {
                string.Format("You shout '{0}'", Subject)
            };

            Message toActor = new Message()
            {
                Override = sb
            };

            string[] areaString = new string[] { string.Format("$A$ shouts '{0}'", Subject) };

            Message toArea = new Message()
            {
                Override = areaString
            };

            //TODO: language outputs
            MessageCluster messagingObject = new MessageCluster(toActor)
            {
                ToOrigin = new List<IMessage> { toArea }
            };

            messagingObject.ExecuteMessaging(Actor, null, null, OriginLocation.CurrentZone, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
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
                return string.Format("Shout communicates in whatever your current language is to the immediate surroundings at an audible strength 30x say/speak. Character with good hearing may be able to hear from further away.");
            }
            set { }
        }
    }
}