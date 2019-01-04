using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.NPC.IntelligenceControl;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Backing data for NPC/Intelligences
    /// </summary>
    public interface INonPlayerCharacterFramework : IHaveHealth, IHaveStamina, IHaveInventoryToSell, IHaveSkillsToTeach
    {
        /// <summary>
        /// Family name for NPCs
        /// </summary>      
        string SurName { get; set; }

        /// <summary>
        /// Gender of the npc
        /// </summary>
        string Gender { get; set; }

        /// <summary>
        /// The race daya for this npc, not its own data structure
        /// </summary>
        IRace Race { get; set; }

        /// <summary>
        /// The matrix of preferences and AI details
        /// </summary>
        IPersonality Personality { get; set; }
    }
}
