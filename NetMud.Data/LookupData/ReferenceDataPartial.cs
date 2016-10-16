using NetMud.Data.System;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.Data.LookupData
{
    public abstract class LookupDataPartial : BackingDataPartial, ILookupData
    {
        public LookupDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        public string HelpText { get; set; }

        public virtual IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }
    }
}
