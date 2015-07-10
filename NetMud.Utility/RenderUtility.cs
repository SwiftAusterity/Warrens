using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace NetMud.Utility
{
    public static class RenderUtility
    {
        private const string encapsulationElement = "p";
        private const string bumperElement = "<br />";

        public static string EncapsulateOutput(IEnumerable<string> lines)
        {
            var returnString = new StringBuilder();

            foreach(var line in lines)
            {
                if (!String.IsNullOrWhiteSpace(line))
                    returnString.AppendFormat("<{0}>{1}</{0}>", encapsulationElement, line);
                else
                    returnString.Append(bumperElement); //blank strings mean carriage returns
            }

            return returnString.ToString();
        }
    }
}
