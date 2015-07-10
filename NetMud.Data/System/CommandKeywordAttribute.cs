using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandKeywordAttribute : Attribute
    {
        public IEnumerable<string> Keywords { get; private set; }

        public CommandKeywordAttribute(string keywords)
        {
            //Way easier just to load them all into lowercase so we don't have to move the cost to runtime
            if (!String.IsNullOrWhiteSpace(keywords))
                this.Keywords = keywords.Split(',').Select(kw => kw.ToLower());
        }
    }
}
