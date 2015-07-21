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

            wssv.AddWebSocketService<Echo>("/");
            wssv.Start();
        }

        public class Echo : WebSocketBehavior
        {
            public ApplicationUserManager UserManager { get; set; }

            private string _userId;

            public Echo()
            {
            }

            public Echo(ApplicationUserManager userManager)
            {
                UserManager = userManager;
            }

            protected override void OnOpen()
            {
                var authTicketValue = Context.CookieCollection[".AspNet.ApplicationCookie"].Value;

                GetUserIDFromCookie(authTicketValue);

                UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
            }

            protected override void OnMessage(MessageEventArgs e)
            {
                var authedUser = UserManager.FindById(_userId);

                var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.ID.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

                if (currentCharacter == null)
                {
                    Send("<p>No character selected</p>");
                    return;
                }

                var player = new Player(currentCharacter);
                player.Descriptor = DataStructure.Base.Entity.DescriptorType.WebSockets;
                player.DescriptorID = ID;
                player.WriteTo = (strings) => SendWrapper(strings);

                //It only sends the errors
                Send(Interpret.Render(e.Data, player));
            }

            public bool SendWrapper(IEnumerable<string> strings)
            {
                Send(RenderUtility.EncapsulateOutput(strings));

                return true;
            }


            private void GetUserIDFromCookie(string authTicketValue)
            {
                authTicketValue = authTicketValue.Replace('-', '+').Replace('_', '/');

                var padding = 3 - ((authTicketValue.Length + 3) % 4);
                if (padding != 0)
                    authTicketValue = authTicketValue + new string('=', padding);

                var bytes = Convert.FromBase64String(authTicketValue);

                bytes = System.Web.Security.MachineKey.Unprotect(bytes,
                    "Microsoft.Owin.Security.Cookies.CookieAuthenticationMiddleware",
                            "ApplicationCookie", "v1");

                using (var memory = new MemoryStream(bytes))
                {
                    using (var compression = new GZipStream(memory, CompressionMode.Decompress))
                    {
                        using (var reader = new BinaryReader(compression))
                        {
                            reader.ReadInt32(); // Ignoring version here
                            string authenticationType = reader.ReadString();
                            reader.ReadString(); // Ignoring the default name claim type
                            reader.ReadString(); // Ignoring the default role claim type

                            int count = reader.ReadInt32(); // count of claims in the ticket

                            var claims = new Claim[count];
                            for (int index = 0; index != count; ++index)
                            {
                                string type = reader.ReadString();
                                type = type == "\0" ? ClaimTypes.Name : type;

                                string value = reader.ReadString();

                                string valueType = reader.ReadString();
                                valueType = valueType == "\0" ?
                                                "http://www.w3.org/2001/XMLSchema#string" : valueType;

                                string issuer = reader.ReadString();
                                issuer = issuer == "\0" ? "LOCAL AUTHORITY" : issuer;

                                string originalIssuer = reader.ReadString();
                                originalIssuer = originalIssuer == "\0" ? issuer : originalIssuer;

                                claims[index] = new Claim(type, value, valueType, issuer, originalIssuer);
                            }

                            var identity = new ClaimsIdentity(claims, authenticationType,
                                                                  ClaimTypes.Name, ClaimTypes.Role);

                            var principal = new ClaimsPrincipal(identity);
                            System.Threading.Thread.CurrentPrincipal = principal;
                            _userId = principal.Identity.GetUserId();
                        }
                    }
                }
            }
        }
    }
}
