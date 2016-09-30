using NetMud.Data.System;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;
using System;

namespace NetMud.Data.Reference
{
    public abstract class ReferenceDataPartial : BackingDataPartial, IReferenceData
    {
        public ReferenceDataPartial()
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
