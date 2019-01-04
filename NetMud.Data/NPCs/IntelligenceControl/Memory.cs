using NetMud.DataStructure.NPC.IntelligenceControl;
using System;
using System.Collections.Generic;

namespace NetMud.Data.NPCs.IntelligenceControl
{
    /// <summary>
    /// A record of NPC observance
    /// </summary>
    [Serializable]
    public class Memory : IMemory
    {
        /// <summary>
        /// When this happened
        /// </summary>
        public DateTime Created { get; set; }

        /// <summary>
        /// What happened
        /// </summary>
        public string Occurrence { get; set; }

        /// <summary>
        /// Who all was involved
        /// </summary>
        public IEnumerable<IReflection> Involved { get; set; }

        /// <summary>
        /// Who was the primary involved, often the character acting
        /// </summary>
        public IReflection Primary { get; set; }

        /// <summary>
        /// Who was the target of the action
        /// </summary>
        public IReflection Target { get; set; }

        /// <summary>
        /// Was I the target?
        /// </summary>
        public bool SelfTarget { get; set; }

        /// <summary>
        /// Was I the initiator?
        /// </summary>
        public bool SelfActor { get; set; }

        /// <summary>
        /// Matrix of Qualities (mapped in Preferences) to developmental impact
        /// </summary>
        public Dictionary<string, short> Feelings { get; set; }
    }
}
