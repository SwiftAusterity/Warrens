using NetMud.DataStructure.Architectural;
using System.Collections.Generic;

namespace NetMud.DataStructure.System
{
    /// <summary>
    /// Interface defining players connected to a server
    /// </summary>
    public interface IDescriptor : ILiveData
    {
        string _userId { get; set; }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        void Open();

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        bool SendOutput(IEnumerable<string> strings);

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        bool SendOutput(string str);

        /// <summary>
        /// Send a sound file to a player
        /// </summary>
        /// <param name="soundUri"></param>
        /// <returns></returns>
        bool SendSound(string soundUri);

        /// <summary>
        /// Disconnects this descriptor forcibly
        /// </summary>
        /// <param name="finalMessage">the final string to send the client</param>
        void Disconnect(string finalMessage);
    }
}
