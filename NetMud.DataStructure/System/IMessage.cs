using NetMud.DataStructure.Linguistic;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    public interface IMessage
    {
        /// <summary>
        /// Message to send to the acting entity
        /// </summary>
        IEnumerable<ILexicalParagraph> Messages { get; set; }

        /// <summary>
        /// Get the string version of all the contained messages
        /// </summary>
        /// <param name="target">The entity type to select the messages of</param>
        /// <returns>Everything unpacked</returns>
        string Unpack(LexicalContext overridingContext = null);
    }
}
