using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NetMud.Communication.Messaging
{
    /// <summary>
    /// Used by the system to produce output for commands and events
    /// </summary>
    [Serializable]
    public class Message : IMessage
    {
        /// <summary>
        /// Message to send to the destination location of the command/event
        /// </summary>
        public IEnumerable<ILexicalParagraph> Messages { get; set; }

        /// <summary>
        /// New up an empty cluster
        /// </summary>
        public Message()
        {
            Messages = Enumerable.Empty<ILexicalParagraph>();
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(ILexicalParagraph messages)
        {
            Messages = new List<ILexicalParagraph>() { messages };
        }

        /// <summary>
        /// New up a clister with just toactor for system messages
        /// </summary>
        public Message(IEnumerable<ILexicalParagraph> messages)
        {
            Messages = messages;
        }

        /// <summary>
        /// Get the string version of all the contained messages
        /// </summary>
        /// <param name="target">The entity type to select the messages of</param>
        /// <returns>Everything unpacked</returns>
        public string Unpack(LexicalContext overridingContext = null)
        {

            return string.Join(" ", Messages.Select(msg => msg.Describe(overridingContext)));
        }

        //TODO: Sentence combinatory logic for lexica output
    }

}
