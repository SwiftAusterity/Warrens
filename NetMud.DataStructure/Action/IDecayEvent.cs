namespace NetMud.DataStructure.Action
{
    /// <summary>
    /// Actions players can take against things with other things
    /// </summary>
    public interface IDecayEvent : IAction
    {
        /// <summary>
        /// The number of seconds this will last for until it fires
        /// </summary>
        int Timer { get; set; }

        /// <summary>
        /// The current time of the timer
        /// </summary>
        int CurrentTime { get; set; }
    }
}
