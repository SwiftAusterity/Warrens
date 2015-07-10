using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class CommandKeywordAttribute : Attribute
    {
        public string Keyword { get; private set; }

        public CommandKeywordAttribute(string keyword)
        {
            //Way easier just to load them all into lowercase so we don't have to move the cost to runtime
            if (!String.IsNullOrWhiteSpace(keyword))
                this.Keyword = keyword.ToLower();
            else
                throw (new ArgumentNullException(String.Format("{0} Command accessor keyword blank on implimentation.", this.GetType().ToString())));
        }
    }
}
