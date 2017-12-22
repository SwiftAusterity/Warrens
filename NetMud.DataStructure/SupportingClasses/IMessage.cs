using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        /// Grammatical subject
        /// </summary>
        string Subject { get; set; }

        /// <summary>
        /// Object of the sentence
        /// </summary>
        string Object { get; set; }

        /// <summary>
        /// Action being taken
        /// </summary>
        string Verb { get; set; }

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
