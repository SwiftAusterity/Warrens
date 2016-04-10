namespace NetMud.Communication
{
    /// <summary>
    /// Types of communication channels
    /// </summary>
    public interface IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        string cacheKeyFormat { get; }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        ConnectionType ConnectedBy { get; }

        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        string EncapsulationElement { get; }

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
       string BumperElement { get; }
    }
}
