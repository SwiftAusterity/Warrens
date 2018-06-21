using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using System;

namespace NetMud.Data.LookupData
{
    /// <summary>
    /// Referred to as Help Files in the UI, extra help content for the help command
    /// </summary>
    [Serializable]
    public class Help : LookupDataPartial, IHelp
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        public override ContentApprovalType ApprovalType { get { return ContentApprovalType.Staff; } }

        /// <summary>
        /// New up a "blank" help entry
        /// </summary>
        public Help()
        {
            Id = -1;
            Name = "NotImpl";
            HelpText = "NotImpl";
        }
    }
}
