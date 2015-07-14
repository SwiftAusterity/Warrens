using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Commands
{
    //Really help can be invoked on anything that is helpful, even itself
    [CommandKeyword("Help")]
    [CommandPermission(StaffRank.Player)]
    [CommandParameter(CommandUsage.Subject, typeof(IHelpful), new CacheReferenceType[] { CacheReferenceType.Help, CacheReferenceType.Code } )] 
    public class Help : ICommand, IHelpful
    {
        private IHelpful Topic;

        public Help()
        {
            //Generic constructor for all IHelpfuls is needed
        }

        public Help(IEnumerable<object> parms)
        {
            if(parms.Count() == 0)
                throw new MethodAccessException("Bad help subject.");

            if (parms.First().GetType().GetInterfaces().Contains(typeof(IHelpful)))
                Topic = parms.First() as IHelpful;
            else
                throw new MethodAccessException("Bad help subject.");
        }

        public Help(IHelpful subject)
        {
            Topic = subject;
        }

        public IEnumerable<string> Execute()
        {
            var sb = GetHelpHeader(Topic);

            sb = sb.Concat(Topic.RenderHelpBody()).ToList();

            //If it's a command render the syntax help at the bottom
            if (Topic.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
               var subject = (ICommand)Topic;
               sb.Add(String.Empty);
               sb = sb.Concat(subject.RenderSyntaxHelp()).ToList();
            }

            return sb;
        }

        public IEnumerable<string> RenderSyntaxHelp()
        {
            var sb = new List<string>();

            sb.Add(String.Format("Valid Syntax: help &lt;topic&gt;"));

            return sb;
        }

        /// <summary>
        /// Renders the help text for the help command itself
        /// </summary>
        /// <returns>string</returns>
        public IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(String.Format("Help provides useful information and syntax for the various commands you can use in the world."));

            return sb;
        }

        private IList<string> GetHelpHeader(IHelpful subject)
        {
            var sb = new List<string>();
            var subjectName = subject.GetType().Name;
            var typeName = "Help";

            if(subject.GetType().GetInterfaces().Contains(typeof(IReference)))
            {
                var refSubject = (IReference)subject;

                subjectName = refSubject.Name;
                typeName = "Reference";
            }
            else if(subject.GetType().GetInterfaces().Contains(typeof(ICommand)))
            {
                typeName = "Commands";
            }

            sb.Add(String.Format("{0} - <span style=\"color: orange\">{1}</span>", typeName, subjectName));
            sb.Add(String.Empty.PadLeft(typeName.Length + 3 + subjectName.Length, '-'));

            return sb;
        }
    }
}
