using NetMud.DataAccess;
using System;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace NetMud.Websock
{
    /// <summary>
    /// A websocket client server
    /// </summary>
    public class Server
    {
        /// <summary>
        /// Start a new websocket server
        /// </summary>
        /// <param name="domain">unused currently</param>
        /// <param name="portNumber">the port to listen on</param>
        /// <returns>The service manage (which you need to hold on to cause there isn't a way to get this back later)</returns>
        public static WebSocketServiceManager StartServer(string domain, int portNumber)
        {
            try
            {
                var wssv = new WebSocketServer(portNumber);

#if DEBUG
                // To change the logging level.
                wssv.Log.Level = LogLevel.Trace;

                // To change the wait time for the response to the WebSocket Ping or Close.
                wssv.WaitTime = TimeSpan.FromSeconds(2);
#endif

                wssv.AddWebSocketService<CommandNegotiator>("/");

                wssv.Start();

                return wssv.WebSocketServices;
            }
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return null;
        }
    }
}
