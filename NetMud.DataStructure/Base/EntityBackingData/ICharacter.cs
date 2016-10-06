using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for player characters
    /// </summary>
    public interface ICharacter : IEntityBackingData, IGender
    {
        /// <summary>
        /// Family name for character
        /// </summary>
        string SurName { get; set; }

        IRace RaceData { get; set; }

        /// <summary>
        /// Account data object unique key
        /// </summary>
        string AccountHandle { get; set; }

        bool StillANoob { get; set; }

        /// <summary>
        /// What account owns this character
        /// </summary>
        IAccount Account { get; }

        /// <summary>
        /// Command permissions for player character
        /// </summary>
        StaffRank GamePermissionsRank { get; set; }

        /// <summary>
        /// Last known location ID for character in live world
        /// </summary>
        string LastKnownLocation { get; set; }

        /// <summary>
        /// System type for Last known location for character in live world
        /// </summary>
        string LastKnownLocationType { get; set; }

        /// <summary>
        /// Given name + surname
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
