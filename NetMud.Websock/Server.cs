using System;

using WebSocketSharp;
using WebSocketSharp.Server;

namespace NetMud.Websock
{
    public class Server
    {
        public static void StartServer(string domain, int portNumber)
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
        }
    }
}
