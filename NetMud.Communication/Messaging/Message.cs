using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
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
        /// Overrides the grammatical generator
        /// </summary>
        public IEnumerable<string> Body { get; set; }

        /// <summary>
        /// General constructor, at least one of the message part strings should be set but none are required
        /// </summary>
        public Message()
        {
            Body = Enumerable.Empty<string>();
        }

        public Message(string body)
        {
            Body = new List<string>() { body };
        }

        public Message(string[] body)
        {
            Body = body;
        }

        /// <summary>
        /// Will this message been seen/heard/etc by the target
        /// </summary>
        /// <returns>if the message will be noticed</returns>
        public bool IsNoticed(IEntity subject, IEntity target, ITile origin)
        {
            //TODO: Other factors in it being seen
            return true;
        }
    }
}
