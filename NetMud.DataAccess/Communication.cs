using System;
using System.Linq;
using System.Runtime.Caching;
using WebSocketSharp.Server;

namespace NetMud.DataAccess
{
    /// <summary>
    /// Negotiation and access to player connection descriptors
    /// </summary>
    public static class Communication
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
        /// The format the cachekeys for the comms objects take
        /// </summary>
        private static string cacheKeyFormat = "LiveWebSocket.{0}";

        /// <summary>
        /// Sends a message to all live descriptors everywhere
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <returns>success status</returns>
        public static bool Broadcast(string message)
        {
            var services = globalCache.Where(keyValuePair => keyValuePair.Value.GetType() == typeof(WebSocketServiceManager)).Select(kvp => (WebSocketServiceManager)kvp.Value);

            foreach(var service in services)
                service.Broadcast(message);

            return true;
        }

        /// <summary>
        /// Sends a message to all live descriptors on a port
        /// </summary>
        /// <param name="message">the message to send</param>
        /// <param name="portNumber">the port of the connection to send to</param>
        /// <returns>success status</returns>
        public static bool Broadcast(string message, int portNumber)
        {
            var service = GetActiveService(portNumber);

            if (service == null)
                return false;

            service.Broadcast(message);

            return true;
        }

        /// <summary>
        /// Registers a live descriptor for a service on a port
        /// </summary>
        /// <param name="service">the service</param>
        /// <param name="portNumber">the port it is listening on</param>
        public static void RegisterActiveService(WebSocketServiceManager service, int portNumber)
        {
            globalCache.AddOrGetExisting(String.Format(cacheKeyFormat, portNumber), service, globalPolicy);
        }

        /// <summary>
        /// Gets an active listener service
        /// </summary>
        /// <param name="portNumber">the port it is listening on</param>
        /// <returns>the service</returns>
        private static WebSocketServiceManager GetActiveService(int portNumber)
        {
            try
            {
                return (WebSocketServiceManager)globalCache[String.Format(cacheKeyFormat, portNumber)];
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return null;
        }
    }
}
