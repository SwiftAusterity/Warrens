using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Linguistic;

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

        /// <summary>
        /// The default language for the system
        /// </summary>
        ILanguage SystemLanguage { get; set; }
    }
}
