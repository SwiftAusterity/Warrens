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
using NetMud.Backup;
using NetMud.DataAccess;


namespace NetMud.Websock
{
    /// <summary>
    /// Main handler of descriptor connections for websockets
    /// </summary>
    public class CommandNegotiator : WebSocketBehavior
    {
        /// <summary>
        /// The user manager for the application, handles authentication from the web
        /// </summary>
        public ApplicationUserManager UserManager { get; set; }

        /// <summary>
        /// User id of connected player
        /// </summary>
        private string _userId;

        /// <summary>
        /// Creates an instance of the command negotiator
        /// </summary>
        public CommandNegotiator()
        {
            //firefox fix
            IgnoreExtensions = true;
        }

        /// <summary>
        /// Creates an instance of the command negotiator with a specified user manager
        /// </summary>
        /// <param name="userManager">the authentication manager from the web</param>
        public CommandNegotiator(ApplicationUserManager userManager)
        {
            //firefox fix
            IgnoreExtensions = true;
            UserManager = userManager;
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        protected override void OnOpen()
        {
            var authTicketValue = Context.CookieCollection[".AspNet.ApplicationCookie"].Value;

            GetUserIDFromCookie(authTicketValue);

            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));
        }

        /// <summary>
        /// Handles when the connection closes
        /// </summary>
        /// <param name="e">events for closing</param>
        protected override void OnClose(CloseEventArgs e)
        {
            base.OnClose(e);
        }

        /// <summary>
        /// Handles when the connection faults
        /// </summary>
        /// <param name="e">events for the error</param>
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            base.OnError(e);
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        protected override void OnMessage(MessageEventArgs e)
        {
            var authedUser = UserManager.FindById(_userId);

            var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.ID.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

            if (currentCharacter == null)
            {
                Send("<p>No character selected</p>");
                return;
            }

            //Try to see if they are already live
            Player newPlayer = LiveCache.Get<Player>(currentCharacter.ID);

            //Check the backup
            if (newPlayer == null)
            {
                var hotBack = new HotBackup(System.Web.Hosting.HostingEnvironment.MapPath("/HotBackup/"));
                newPlayer = hotBack.RestorePlayer(currentCharacter.AccountHandle, currentCharacter.ID);
            }

            //else new them up
            if (newPlayer == null)
                newPlayer = new Player(currentCharacter);

            newPlayer.Descriptor = DataStructure.Base.Entity.DescriptorType.WebSockets;
            newPlayer.DescriptorID = ID;
            newPlayer.WriteTo = (strings) => SendWrapper(strings);

            var errors = Interpret.Render(e.Data, newPlayer);

            //It only sends the errors
            if (!string.IsNullOrWhiteSpace(errors))
                Send(errors);
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(IEnumerable<string> strings)
        {
            Send(RenderUtility.EncapsulateOutput(strings));

            return true;
        }

        /// <summary>
        /// Gets the user ID from the web from the aspnet cookie
        /// </summary>
        /// <param name="authTicketValue">the cookie's value</param>
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
                        _userId = principal.Identity.GetUserId();
                    }
                }
            }
        }
    }
}
