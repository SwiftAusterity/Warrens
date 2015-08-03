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
    public class Help : ICommand, IHelpful
    {
        /// <summary>
        /// The entity invoking the command
        /// </summary>
        public IActor Actor { get; set; }

        /// <summary>
        /// The entity the command refers to
        /// </summary>
        public object Subject { get; set; }

        /// <summary>
        /// When there is a predicate parameter, the entity that is being targetting (subject become "with")
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Any tertiary entity being referenced in command parameters
        /// </summary>
        public object Supporting { get; set; }

        /// <summary>
        /// Container the Actor is in when the command is invoked
        /// </summary>
        public ILocation OriginLocation { get; set; }

        /// <summary>
        /// Valid containers by range from OriginLocation
        /// </summary>
        public IEnumerable<ILocation> Surroundings { get; set; }

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
        public void Execute()
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

            var messagingObject = new MessageCluster(RenderUtility.EncapsulateOutput(sb), string.Empty, string.Empty, string.Empty, string.Empty);

            messagingObject.ExecuteMessaging(Actor, null, null, null, null);
        }

        /// <summary>
        /// Renders syntactical help for the command, invokes automatically when syntax is bungled
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderSyntaxHelp()
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

            if (subject.GetType().GetInterfaces().Contains(typeof(IReference)))
            {
                var refSubject = (IReference)subject;

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
