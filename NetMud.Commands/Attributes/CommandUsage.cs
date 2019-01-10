namespace NetMud.Commands.Attributes
{
    /// <summary>
    /// What does this command want to do with this parameter
    /// </summary>
    public enum CommandUsage : short
    {
        Subject = 0,
        Target = 1,
        Supporting = 2
    }
}
