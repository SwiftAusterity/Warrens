using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    /// <summary>
    /// Backing data for NPC/Intelligences
    /// </summary>
    public interface INonPlayerCharacter : IEntityBackingData, IGender
    {
        /// <summary>
        /// Family name for NPCs
        /// </summary>
        string SurName { get; set; }

        /// <summary>
        /// The race type for this character
        /// </summary>
        IRace RaceData { get; set; }

        /// <summary>
        /// Given + family name for NPCs
        /// </summary>
        /// <returns></returns>
        string FullName();
    }
}
