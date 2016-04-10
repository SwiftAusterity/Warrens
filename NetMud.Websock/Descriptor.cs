using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.Communication;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.Interp;
using NetMud.Models;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Claims;
using WebSocketSharp;

namespace NetMud.Websock
{
    /// <summary>
    /// Main handler of descriptor connections for websockets
    /// </summary>
    public class Descriptor : Channel, IDescriptor
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
        /// The player connected
        /// </summary>
        private Player _currentPlayer;

        /// <summary>
        /// Creates an instance of the command negotiator
        /// </summary>
        public Descriptor()
        {
            //firefox fix
            IgnoreExtensions = true;
        }

        /// <summary>
        /// Creates an instance of the command negotiator with a specified user manager
        /// </summary>
        /// <param name="userManager">the authentication manager from the web</param>
        public Descriptor(ApplicationUserManager userManager)
        {
            //firefox fix
            IgnoreExtensions = true;
            UserManager = userManager;
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        void IDescriptor.OnOpen()
        {
            OnOpen();
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        protected override void OnOpen()
        {
            var authTicketValue = Context.CookieCollection[".AspNet.ApplicationCookie"].Value;

            GetUserIDFromCookie(authTicketValue);

            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            var authedUser = UserManager.FindById(_userId);

            var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.ID.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

            if (currentCharacter == null)
            {
                Send("<p>No character selected</p>");
                return;
            }

            //Try to see if they are already live
            _currentPlayer = LiveCache.Get<Player>(currentCharacter.ID);

            //Check the backup
            if (_currentPlayer == null)
            {
                var hotBack = new HotBackup(System.Web.Hosting.HostingEnvironment.MapPath("/HotBackup/"));
                _currentPlayer = hotBack.RestorePlayer(currentCharacter.AccountHandle, currentCharacter.ID);
            }

            //else new them up
            if (_currentPlayer == null)
                _currentPlayer = new Player(currentCharacter);

            _currentPlayer.Descriptor = DataStructure.Base.Entity.DescriptorType.WebSockets;
            _currentPlayer.DescriptorID = ID;
            _currentPlayer.WriteTo = (strings) => SendWrapper(strings);
            _currentPlayer.CloseConnection = () =>
            {
                this.Context.WebSocket.Close(CloseStatusCode.Normal, "user exited");
                return true;
            };

            //We need to barf out to the connected client the welcome message. The client will only indicate connection has been established.
            var welcomeMessage = new List<String>();

            welcomeMessage.Add(string.Format("Welcome to alpha phase twinMUD, {0}", currentCharacter.FullName()));
            welcomeMessage.Add(string.Format("Please feel free to LOOK around.", currentCharacter.FullName()));

            SendWrapper(welcomeMessage);

            //Send the look command in
            Interpret.Render("look", _currentPlayer);
        }

        /// <summary>
        /// Handles when the connection closes
        /// </summary>
        /// <param name="e">events for closing</param>
        public void OnClose(object closeArguments)
        {
            OnClose((CloseEventArgs)closeArguments);
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
        public void OnError(object errorArguments)
        {
            OnError((WebSocketSharp.ErrorEventArgs)errorArguments);
        }

        /// <summary>
        /// Handles when the connection faults
        /// </summary>
        /// <param name="e">events for the error</param>
        protected override void OnError(WebSocketSharp.ErrorEventArgs e)
        {
            //Log it
            LoggingUtility.LogError(e.Exception);

            //Do the base behavior from the websockets library
            base.OnError(e);
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        public void OnMessage(object messageArguments)
        {
            OnMessage((MessageEventArgs)messageArguments);
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        protected override void OnMessage(MessageEventArgs e)
        {
            if (_currentPlayer == null)
            {
                SendWrapper("Invalid character; please reload the client and try again.");
                this.Context.WebSocket.Close(CloseStatusCode.Abnormal, "connection aborted - no player"); ;
            }

            var errors = Interpret.Render(e.Data, _currentPlayer);

            //It only sends the errors
            if (errors.Any(str => !string.IsNullOrWhiteSpace(str)))
                SendWrapper(errors);
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(IEnumerable<string> strings)
        {
            Send(RenderUtility.EncapsulateOutput(strings, EncapsulationElement, BumperElement));

            return true;
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(string str)
        {
            Send(str.EncapsulateOutput(EncapsulationElement, BumperElement));

            return true;
        }

        public void Disconnect(string finalMessage)
        {
            Send(finalMessage.EncapsulateOutput(EncapsulationElement, BumperElement));
            base.OnClose(null);
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
