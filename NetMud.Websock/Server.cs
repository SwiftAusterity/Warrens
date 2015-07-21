using System;
using System.Linq;

using NetMud.Data.Game;
using NetMud.Interp;

using WebSocketSharp;
using WebSocketSharp.Server;
using NetMud.Authentication;

using Microsoft.AspNet.Identity;
using System.Security.Claims;
using System.IO;
using System.IO.Compression;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Models;
using NetMud.Utility;
using System.Collections.Generic;

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
