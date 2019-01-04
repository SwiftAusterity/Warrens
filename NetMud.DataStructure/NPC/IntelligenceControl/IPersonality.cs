using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    /// <summary>
    /// The matrix of preferences and AI details
    /// </summary>
    public interface IPersonality
    {
        [UIHint("IPreferenceList")]
        HashSet<IPreference> Preferences { get; set; }

        HashSet<IMemory> Memories { get; set; }
    }
}
