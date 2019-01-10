using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// Singular message object for output parsing
    /// </summary>
    [Serializable]
    public class Message : IMessage
    {
        /// <summary>
        /// Type of output
        /// </summary>
        public MessagingType Type { get; set; }

        /// <summary>
        /// The composed message and strength
        /// </summary>
        public IOccurrence Occurrence { get; set; }

        /// <summary>
        /// Overrides the grammatical generator
        /// </summary>
        public IEnumerable<string> Override { get; set; }

        public Message()
        {
            Type = MessagingType.Visible;

            Override = Enumerable.Empty<string>();
        }

        public Message(string over)
        {
            Type = MessagingType.Visible;

            Override = new string[] { over };
        }

        public Message(string[] over)
        {
            Type = MessagingType.Visible;

            Override = over;
        }

        /// <summary>
        /// General constructor, at least one of the message part strings should be set but none are required
        /// </summary>
        /// <param name="type"></param>
        /// <param name="occurrence"></param>
        public Message(MessagingType type, IOccurrence occurrence)
        {
            Type = type;
            Occurrence = occurrence;

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
