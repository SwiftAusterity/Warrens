namespace NetMud.DataStructure.Behaviors.System
{
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
