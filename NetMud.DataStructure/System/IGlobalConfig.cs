using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Player;

namespace NetMud.DataStructure.System
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
        /// Are new users allowed to register
        /// </summary>
        bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        bool AdminsOnly { get; set; }

        /// <summary>
        /// Config to handle player death
        /// </summary>
        IDeathConfig DeathSettings { get; set; }
    }
}
