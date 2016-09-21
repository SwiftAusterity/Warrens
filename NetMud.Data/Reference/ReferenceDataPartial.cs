using NetMud.Data.System;
using NetMud.DataAccess; using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NetMud.Data.Reference
{
    public abstract class ReferenceDataPartial : SerializableDataPartial, IReferenceData
    {
        public ReferenceDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        public abstract IEnumerable<string> RenderHelpBody();
    }
}
