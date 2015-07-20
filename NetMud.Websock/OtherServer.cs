using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace NetMud.Websock
{
    public class OtherServer
    {
        public static void StartServer(string domain, int portNumber)
        {
            using (var nf = new Notifier())
            using (var ws = new WebSocket(String.Format("ws://{0}:{1}/", domain, portNumber)))
            {
                // To set the WebSocket events.
                ws.OnOpen += (sender, e) => ws.Send("Hi, there!");

                ws.OnMessage += (sender, e) =>
                  nf.Notify(
                    new NotificationMessage
                    {
                        Summary = "WebSocket Message",
                        Body = e.Data,
                        Icon = "notification-message-im"
                    });

                ws.OnError += (sender, e) =>
                  nf.Notify(
                    new NotificationMessage
                    {
                        Summary = "WebSocket Error",
                        Body = e.Message,
                        Icon = "notification-message-im"
                    });

                ws.OnClose += (sender, e) =>
                  nf.Notify(
                    new NotificationMessage
                    {
                        Summary = String.Format("WebSocket Close ({0})", e.Code),
                        Body = e.Reason,
                        Icon = "notification-message-im"
                    });

#if DEBUG
                ws.Log.Level = LogLevel.Trace;

                ws.WaitTime = TimeSpan.FromSeconds(10);
#endif
                // To enable the Per-message Compression extension.
                ws.Compression = CompressionMethod.Deflate;

                /* To validate the server certificate.
                ws.SslConfiguration.ServerCertificateValidationCallback =
                  (sender, certificate, chain, sslPolicyErrors) => {
                    ws.Log.Debug (
                      String.Format (
                        "Certificate:\n- Issuer: {0}\n- Subject: {1}",
                        certificate.Issuer,
                        certificate.Subject));

                    return true;
                  };
                */
                // To set the credentials for the HTTP Authentication (Basic/Digest).
                //ws.SetCredentials ("nobita", "password", false);

                // To send the Cookies.
                //ws.SetCookie (new Cookie ("name", "nobita"));
                //ws.SetCookie (new Cookie ("roles", "\"idiot, gunfighter\""));

                // Connect to the server asynchronously.
                ws.ConnectAsync();

                /*
                    Console.WriteLine ("\nType 'exit' to exit.\n");
                    while (true) 
                    {
                      Thread.Sleep (1000);
                      Console.Write ("> ");

                      var msg = Console.ReadLine();

                      if (msg == "exit")
                        break;

                      // Send a text message.
                      ws.Send (msg);
                    }
                */
            }
        }
    }

}
