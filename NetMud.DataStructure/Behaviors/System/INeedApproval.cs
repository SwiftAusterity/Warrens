using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;

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
        /// The creator's account permissions level
        /// </summary>
        StaffRank CreatorRank { get; set; }

        /// <summary>
        /// Has this been approved?
        /// </summary>
        ApprovalState State { get; set; }

        /// <summary>
        /// Is this able to be seen and used for live purposes
        /// </summary>
        bool SuitableForUse { get; }

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

        /// <summary>
        /// The approver's account permissions level
        /// </summary>
        StaffRank ApproverRank { get; set; }

        /// <summary>
        /// Can the given rank approve this or not
        /// </summary>
        /// <param name="rank">Approver's rank</param>
        /// <returns>If it can</returns>
        bool CanIBeApprovedBy(StaffRank rank, IAccount approver);

        /// <summary>
        /// Change the approval status of this thing
        /// </summary>
        /// <returns>success</returns>
        bool ChangeApprovalStatus(IAccount approver, StaffRank rank, ApprovalState newState);

        /// <summary>
        /// Get the significant details of what needs approval
        /// </summary>
        /// <returns>A list of strings</returns>
        IDictionary<string, string> SignificantDetails();
    }
}
