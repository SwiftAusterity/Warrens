using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Interface defining players connected to a server
    /// </summary>
    public interface IDescriptor
    {
        /// <summary>
        /// The cache key for the global cache system
        /// </summary>
        ICacheKey CacheKey { get; }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        void Open();

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        bool SendWrapper(IEnumerable<string> strings);

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        bool SendWrapper(string str);

        /// <summary>
        /// Disconnects this descriptor forcibly
        /// </summary>
        /// <param name="finalMessage">the final string to send the client</param>
        void Disconnect(string finalMessage);
    }
}
