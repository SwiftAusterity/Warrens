using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Global settings for the entire system
    /// </summary>
    public interface IGlobalConfig : IConfigData
    {
        /// <summary>
        /// Is the websockets portal allowing new connections
        /// </summary>
        bool WebsocketPortalActive { get; set; }
    }
}
