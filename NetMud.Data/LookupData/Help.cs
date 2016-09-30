using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Referred to as Help Files in the UI, extra help content for the help command
    /// </summary>
    [Serializable]
    public class Help : LookupDataPartial, IHelp
    {
        /// <summary>
        /// New up a "blank" help entry
        /// </summary>
        public Help()
        {
            ID = -1;
            Created = DateTime.UtcNow;
            LastRevised = DateTime.UtcNow;
            Name = "NotImpl";
            HelpText = "NotImpl";
        }
    }
}
