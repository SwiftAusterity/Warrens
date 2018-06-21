using NetMud.DataStructure.Base.System;
using System;

namespace NetMud.DataStructure.Behaviors.System
{
    /// <summary>
    /// For content that requires approval before being shown live. Approval is compared against creator's level
    /// </summary>
    public interface INeedApproval
    {
        /// <summary>
        /// What type of approval is necessary for this content
        /// </summary>
        ContentApprovalType ApprovalType { get; }

        /// <summary>
        /// Who created this thing, their GlobalAccountHandle
        /// </summary>
        string CreatorHandle { get; set; }

        /// <summary>
        /// Who created this thing
        /// </summary>
        IAccount Creator { get; set; }

        /// <summary>
        /// Has this been approved?
        /// </summary>
        bool Approved { get; set; }

        /// <summary>
        /// When was this approved
        /// </summary>
        DateTime ApprovedOn { get; set; }

        /// <summary>
        /// Who approved this thing, their GlobalAccountHandle
        /// </summary>
        string ApproverHandle { get; set; }

        /// <summary>
        /// Who approved this thing
        /// </summary>
        IAccount ApprovedBy { get; set; }
    }

    /// <summary>
    /// Types of approval necessary for content to be made live.
    /// </summary>
    public enum ContentApprovalType
    {
        None, //Doesn't even get shown in the ui for the approval system
        ReviewOnly, //Gets tossed in the review list just to make it seen
        Leader, //for eventual guild content changing
        Staff, //Any staff can approve
        Admin //Highest admin rank approval required
    }
}
