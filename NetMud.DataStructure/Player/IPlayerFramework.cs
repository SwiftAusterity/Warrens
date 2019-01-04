using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.ActorBase;

namespace NetMud.DataStructure.Player
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface IPlayerFramework : IHaveHealth, IHaveStamina
    {
        /// <summary>
        /// Command permissions for player character
        /// </summary>
        StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// Family name for character
        /// </summary>     
        string SurName { get; set; }

        /// <summary>
        /// Gender of the npc
        /// </summary>
        string Gender { get; set; }

        /// <summary>
        /// Is this character not graduated from the tutorial
        /// </summary>
        bool StillANoob { get; set; }

        /// <summary>
        /// Sensory overrides for staff member characters
        /// </summary>
        bool SuperVision { get; set; }
    }
}
