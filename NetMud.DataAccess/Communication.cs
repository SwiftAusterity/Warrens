using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp.Server;

namespace NetMud.DataAccess
{
    public static class Communication
    {
        private static ObjectCache globalCache = MemoryCache.Default;
        private static CacheItemPolicy globalPolicy = new CacheItemPolicy();
        private static string cacheKeyFormat = "LiveWebSocket.{0}";

        public static bool Broadcast(string message)
        {
            var services = globalCache.Where(keyValuePair => keyValuePair.Value.GetType() == typeof(WebSocketServiceManager)).Select(kvp => (WebSocketServiceManager)kvp.Value);

            foreach(var service in services)
                service.Broadcast(message);

            return true;
        }

        public static bool Broadcast(string message, int portNumber)
        {
            var service = GetActiveService(portNumber);

            if (service == null)
                return false;

            service.Broadcast(message);

            return true;
        }

        public static void RegisterActiveService(WebSocketServiceManager service, int portNumber)
        {
            globalCache.AddOrGetExisting(String.Format(cacheKeyFormat, portNumber), service, globalPolicy);
        }

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
