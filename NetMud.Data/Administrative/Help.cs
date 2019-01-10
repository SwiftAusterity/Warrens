using NetMud.Data.Architectural;
using NetMud.DataStructure.Administrative;
using System;

namespace NetMud.Data.Administrative
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

        /// <summary>
        /// Make a copy of this
        /// </summary>
        /// <returns>A copy</returns>
        public override object Clone()
        {
            return new Help
            {
                Name = Name,
                HelpText = HelpText
            };
        }
    }
}
