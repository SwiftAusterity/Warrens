using NetMud.Data.DataIntegrity;
using NetMud.Data.System;
using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Partial for anything that is considered lookup data (non-entity backing data)
    /// </summary>
    public abstract class LookupDataPartial : BackingDataPartial, ILookupData
    {
        /// <summary>
        /// Extra text for the help command to display
        /// </summary>
        [StringDataIntegrity("Help text empty.", warning: true)]
        public string HelpText { get; set; }

        /// <summary>
        /// Make a new one of these
        /// </summary>
        public LookupDataPartial()
        {
            //empty instance for getting the dataTableName
        }

        /// <summary>
        /// Render out the display for the help command
        /// </summary>
        /// <returns>Help text</returns>
        public virtual IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>
            {
                HelpText
            };

            return sb;
        }
    }
}
