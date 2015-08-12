using NetMud.Commands.Attributes;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Utility;
using NutMud.Commands.Attributes;
using System.Collections.Generic;
using System.Linq;

namespace NutMud.Commands.System
{
    /// <summary>
    /// Displays help text for a help file (data) or command (RenderHelpBody)
    /// </summary>
    [CommandKeyword("Help", false)]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IHelpful), new CacheReferenceType[] { CacheReferenceType.Help, CacheReferenceType.Code }, false)]
    public class Help : CommandPartial, IHelpful
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
            var sb = GetHelpHeader(topic);

            sb = sb.Concat(topic.RenderHelpBody()).ToList();

            //If it's a command render the syntax help at the bottom
            if (topic.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
                var subject = (ICommand)topic;
                sb.Add(string.Empty);
                sb = sb.Concat(subject.RenderSyntaxHelp()).ToList();
            }

            var messagingObject = new MessageCluster(sb, new string[] { }, new string[] { }, new string[] { }, new string[] { });

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public override IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Valid Syntax: help &lt;topic&gt;"));

            return sb;
        }

        /// <summary>
        /// Renders the help text for the help command itself
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(string.Format("Help provides useful information and syntax for the various commands you can use in the world."));

            return sb;
        }

        private IList<string> GetHelpHeader(IHelpful subject)
        {
            var sb = new List<string>();
            var subjectName = subject.GetType().Name;
            var typeName = "Help";

            if (subject.GetType().GetInterfaces().Contains(typeof(IReferenceData)))
            {
                var refSubject = (IReferenceData)subject;

                subjectName = refSubject.Name;
                typeName = "Reference";
            }
            else if (subject.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
                typeName = "Commands";
            }

            sb.Add(string.Format("{0} - <span style=\"color: orange\">{1}</span>", typeName, subjectName));
            sb.Add(string.Empty.PadLeft(typeName.Length + 3 + subjectName.Length, '-'));

            return sb;
        }
    }
}
