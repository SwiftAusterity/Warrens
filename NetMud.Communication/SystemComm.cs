using System;
using System.Linq;
using System.Runtime.Caching;

namespace NetMud.Communication
{
    /// <summary>
    /// Negotiation and access to ALL player connection descriptors
    /// </summary>
    public static class SystemComm
    {
        /// <summary>
        /// The place everything gets stored
        /// </summary>
        private static ObjectCache globalCache = MemoryCache.Default;

        /// <summary>
        /// The general storage policy
        /// </summary>
        private static CacheItemPolicy globalPolicy = new CacheItemPolicy();

        /// <summary>
        /// Sends a message to all live descriptors everywhere
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>success status</returns>
        public static bool BroadcastToAll(string message)
        {
            //TODO: Make these find all servers cached and send to them
            return true;
        }

        /// <summary>
        /// Sends a message to all live descriptors on a port
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="portNumber">the port of the connection to send to</param>
        /// <returns>success status</returns>
        public static bool BroadcastToAll(string message, int portNumber)
        {
            //TODO: Make these find all servers of this port cached and send to them
            return true;
        }
    }
}
