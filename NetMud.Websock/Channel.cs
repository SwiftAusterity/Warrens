using NetMud.Communication;
using WebSocketSharp.Server;

namespace NetMud.Websock
{
    /// <summary>
    /// Partial class for websocket channel details
    /// </summary>
    public abstract class Channel : WebSocketBehavior, IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat { get { return "LiveWebSocket.{0}"; } }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy { get { return ConnectionType.Websocket; } }

        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        public string EncapsulationElement { get { return "<p>"; } }

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
        public string BumperElement { get { return "<br />"; } }
    }
}
