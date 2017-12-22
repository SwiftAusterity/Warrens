using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// Singular message object for output parsing
    /// </summary>
    public class Message : IMessage
    {
        /// <summary>
        /// Type of output
        /// </summary>
        public MessagingType Type { get; set; }

        /// <summary>
        /// Quality of the output to be graded against sensory ability and environmental factors
        /// </summary>
        public int Strength { get; set; }

        /// <summary>
        /// Grammatical subject
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Object of the sentence
        /// </summary>
        public string Object { get; set; }

        /// <summary>
        /// Action being taken
        /// </summary>
        public string Verb { get; set; }

        /// <summary>
        /// Overrides the grammatical generator
        /// </summary>
        public IEnumerable<string> Override { get; set; }

        /// <summary>
        /// General constructor, at least one of the message part strings should be set but none are required
        /// </summary>
        /// <param name="type"></param>
        /// <param name="strength"></param>
        public Message(MessagingType type, int strength)
        {
            Type = type;
            Strength = strength;

            Override = Enumerable.Empty<string>();
        }

        /// <summary>
        /// Will this message been seen/heard/etc by the target
        /// </summary>
        /// <returns>if the message will be noticed</returns>
        public bool IsNoticed(IEntity subject, IEntity target, ILocation origin)
        {
            //TODO: Other factors in it being seen
            return true;
        }
    }
}
