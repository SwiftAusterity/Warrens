namespace NetMud.DataStructure.Administrative
{
    /// <summary>
    /// Approval states for content
    /// </summary>
    public enum ApprovalState : short
    {
        Unapproved = 0,
        Pending = 1,
        Returned = 2,
        Approved = 3
    }
}
