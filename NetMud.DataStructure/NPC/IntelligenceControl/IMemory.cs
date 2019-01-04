using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.NPC.IntelligenceControl
{
    /// <summary>
    /// A record of NPC observance
    /// </summary>
    public interface IMemory
    {
        /// <summary>
        /// When this happened
        /// </summary>
        DateTime Created { get; set; }

        /// <summary>
        /// What happened
        /// </summary>
        string Occurrence { get; set; }

        /// <summary>
        /// Who all was involved
        /// </summary>
        IEnumerable<IReflection> Involved { get; set; }

        /// <summary>
        /// Who was the primary involved, often the character acting
        /// </summary>
        IReflection Primary { get; set; }

        /// <summary>
        /// Who was the target of the action
        /// </summary>
        IReflection Target { get; set; }

        /// <summary>
        /// Was I the target?
        /// </summary>
        bool SelfTarget { get; set; }

        /// <summary>
        /// Was I the initiator?
        /// </summary>
        bool SelfActor { get; set; }

        /// <summary>
        /// Matrix of Qualities (mapped in Preferences) to developmental impact
        /// </summary>
        Dictionary<string, short> Feelings { get; set; }
    }
}
