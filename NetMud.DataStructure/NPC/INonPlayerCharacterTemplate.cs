using NetMud.DataStructure.Action;
using NetMud.DataStructure.Architectural.EntityBase;
using System.Collections.Generic;

namespace NetMud.DataStructure.NPC
{
    /// <summary>
    /// Backing data for NPC/Intelligences
    /// </summary>
    public interface INonPlayerCharacterTemplate : ITemplate, INonPlayerCharacterFramework
    {
        /// <summary>
        /// Given + family name for NPCs
        /// </summary>
        /// <returns></returns>
        string FullName();

        /// <summary>
        /// Character->tile interactions
        /// </summary>
        HashSet<IUse> UsableAbilities { get; set; }
    }
}
