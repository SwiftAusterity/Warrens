using NetMud.Data.Base.System;
using NetMud.Data.Behaviors.Rendering;
using NetMud.Data.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Commands
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

            return sb.Concat(Topic.RenderHelpBody()); ;
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
            sb.Add(String.Empty);
            sb.Add(String.Format("Valid Syntax: "));
            sb.Add(String.Format("help &lt;topic&gt;"));

            return sb;
        }

        private IEnumerable<string> GetHelpHeader(IHelpful subject)
        {
            var sb = new List<string>();
            var subjectName = subject.GetType().Name;
            var titleLine = String.Format("Help - <span style=\"color: orange\">{0}</span>", subjectName);

            sb.Add(titleLine);
            sb.Add(String.Empty.PadLeft(7 + subjectName.Length, '-'));

            return sb;
        }
    }
}
