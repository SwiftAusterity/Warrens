using NetMud.DataStructure.NPC.IntelligenceControl;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.NPC.IntelligenceControl
{
    /// <summary>
    /// The matrix of preferences and AI details
    /// </summary>
    [Serializable]
    public class Personality : IPersonality
    {
        [UIHint("PreferenceList")]
        public HashSet<IPreference> Preferences { get; set; }

        public HashSet<IMemory> Memories { get; set; }

        public Personality()
        {
            Preferences = new HashSet<IPreference>();
            Memories = new HashSet<IMemory>();
        }
    }
}
