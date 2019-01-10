using NetMud.Commands.Attributes;
using NetMud.Communication.Messaging;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Commands.System
{
    /// <summary>
    /// Displays help text for a help file (data) or command (RenderHelpBody)
    /// </summary>
    [CommandKeyword("Help", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IHelpful), new CacheReferenceType[] { CacheReferenceType.Help, CacheReferenceType.Code }, false)]
    public class Help : CommandPartial
    {
        /// <summary>
        /// All Commands require a generic constructor
        /// </summary>
        public Help()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        /// <summary>
        /// Executes this command
        /// </summary>
        public override void Execute()
        {
            var topic = (IHelpful)Subject;
            IList<string> sb = GetHelpHeader(topic);

            sb = sb.Concat(topic.RenderHelpBody()).ToList();

            //If it's a command render the syntax help at the bottom
            if (topic.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
                var subject = (ICommand)topic;
                sb.Add(string.Empty);
                sb = sb.Concat(subject.RenderSyntaxHelp()).ToList();
            }

            Message toActor = new Message()
            {
                Override = sb
            };

            MessageCluster messagingObject = new MessageCluster(toActor);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            List<string> sb = new List<string>
            {
                string.Format("Valid Syntax: help &lt;topic&gt;")
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
                return string.Format("Help provides useful information and syntax for the various commands you can use in the world.");
            }
            set {  }
        }

        private IList<string> GetHelpHeader(IHelpful subject)
        {
            List<string> sb = new List<string>();
            var subjectName = subject.GetType().Name;
            string typeName = "Help";

            if (subject.GetType().GetInterfaces().Contains(typeof(ILookupData)))
            {
                var refSubject = (ILookupData)subject;

                subjectName = refSubject.Name;
                typeName = "Lookup Data";
            }
            else if (subject.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
                typeName = "Commands";
            }

            sb.Add(string.Format("{0} - %O%{1}%O%", typeName, subjectName));
            sb.Add(string.Empty.PadLeft(typeName.Length + 3 + subjectName.Length, '-'));

            return sb;
        }
    }
}
