using NetMud.Communication;

namespace NetMud.Telnet
{
    /// <summary>
    /// Partial class for telnet channel details
    /// </summary>
    public abstract class Channel : IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat { get { return "LiveTelnet.{0}"; } }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy { get { return ConnectionType.Telnet; } }

        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        public string EncapsulationElement { get { return string.Empty; } }

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
        public string BumperElement { get { return "\n\r"; } }
    }
}
