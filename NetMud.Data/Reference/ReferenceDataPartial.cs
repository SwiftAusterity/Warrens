using NetMud.Data.System;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.Data.Reference
{
    public abstract class ReferenceDataPartial : BackingDataPartial, IReferenceData
    {
        public ReferenceDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        public abstract IEnumerable<string> RenderHelpBody();
    }
}
