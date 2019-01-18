using System.Collections.Generic;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    /// <summary>
    /// The matrix of preferences and AI details
    /// </summary>
    public interface IPersonality
    {
        HashSet<IPreference> Preferences { get; set; }

        HashSet<IMemory> Memories { get; set; }
    }
}
