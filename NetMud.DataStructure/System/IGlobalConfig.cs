using NetMud.DataStructure.Architectural;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Global settings for the entire system
    /// </summary>
    public interface IGlobalConfig : IConfigData
    {
        /// <summary>
        /// Are new users allowed to register
        /// </summary>
        bool UserCreationActive { get; set; }

        /// <summary>
        /// Are only admins allowed to log in - noone at StaffRank.Player
        /// </summary>
        bool AdminsOnly { get; set; }
    }
}
