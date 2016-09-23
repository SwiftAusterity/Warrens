using NetMud.DataAccess; using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace NetMud.Data.Reference
{
    /// <summary>
    /// Referred to as Help Files in the UI, extra help content for the help command
    /// </summary>
    [Serializable]
    public class Help : ReferenceDataPartial, IHelp
    {
        /// <summary>
        /// Help text for the body of the render to help command
        /// </summary>
        public string HelpText { get; set; }

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

        /// <summary>
        /// Renders the help text for this data object
        /// </summary>
        /// <returns>help text</returns>
        public override IEnumerable<string> RenderHelpBody()
        {
            var sb = new List<string>();

            sb.Add(HelpText);

            return sb;
        }
    }
}
