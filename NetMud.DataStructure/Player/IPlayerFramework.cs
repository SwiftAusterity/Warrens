using NetMud.DataStructure.Administrative;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface IPlayerFramework 
    {
        /// <summary>
        /// Account this player belongs to
        /// </summary>
        string AccountHandle { get; set; }

        /// <summary>
        /// Command permissions for player character
        /// </summary>
        StaffRank GamePermissionsRank { get; set; }
    }
}
