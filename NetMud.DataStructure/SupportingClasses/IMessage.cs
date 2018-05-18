using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System.Collections.Generic;

namespace NetMud.DataStructure.SupportingClasses
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
        /// Quality of the output to be graded against sensory ability and environmental factors
        /// </summary>
        int Strength { get; set; }

        /// <summary>
        /// The composed message
        /// </summary>
        ILexica Lexica { get; set; }

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
