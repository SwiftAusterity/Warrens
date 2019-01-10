using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Singular message object for output parsing
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Type of output
        /// </summary>
        MessagingType Type { get; set; }

        /// <summary>
        /// The composed message and strength
        /// </summary>
        IOccurrence Occurrence { get; set; }

        /// <summary>
        /// Overrides the grammatical generator
        /// </summary>
        IEnumerable<string> Override { get; set; }

        /// <summary>
        /// Will this message been seen/heard/etc by the target
        /// </summary>
        /// <returns>if the message will be noticed</returns>
        bool IsNoticed(IEntity subject, IEntity target, ILocation origin);
    }
}
