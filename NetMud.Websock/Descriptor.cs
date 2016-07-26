using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.Communication;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.Interp;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace NetMud.Websock
{
    /// <summary>
    /// Main handler of descriptor connections for websockets
    /// </summary>
    public class Descriptor : Channel, IDescriptor
    {
        /// <summary>
        /// For Websocket handshaking
        /// </summary>
        static private string guid = "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";

        /// <summary>
        /// The user manager for the application, handles authentication from the web
        /// </summary>
        public ApplicationUserManager UserManager { get; set; }
        internal TcpClient Client { get; set; }
        private static byte[] data = new byte[dataSize];
        private const int dataSize = 1024;

        /// <summary>
        /// The cache key for the global cache system
        /// </summary>
        public string CacheKey
        {
            get
            {
                return "WebSocketDescriptor_" + Client.Client.RemoteEndPoint.Serialize().ToString();
            }
        }

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
        public Descriptor(TcpClient tcpClient)
        {
            Client = tcpClient;
            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            LiveCache.Add(this, String.Format(cacheKeyFormat, CacheKey));

            //StartLoop(OnOpen);
            OnOpen();
        }

        /// <summary>
        /// Creates an instance of the command negotiator with a specified user manager
        /// </summary>
        /// <param name="userManager">the authentication manager from the web</param>
        public Descriptor(TcpClient tcpClient, ApplicationUserManager userManager)
        {
            Client = tcpClient;
            UserManager = userManager;

            LiveCache.Add(this, String.Format(cacheKeyFormat, CacheKey));

            //StartLoop(OnOpen);
            OnOpen();
        }

        private async void StartLoop(Func<bool> worker)
        {
            await Task.Delay(50);
            var success = worker.Invoke();

            if (success)
                StartLoop(worker);
            else
                OnClose();
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        public bool OnOpen()
        {
            NetworkStream stream = Client.GetStream();

            //enter to an infinite cycle to be able to handle every change in stream
            while (!stream.DataAvailable)
                ;

            var bytes = new Byte[Client.Available];

            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            var data = Encoding.UTF8.GetString(bytes);

            //initial connection
            if (new Regex("^GET").IsMatch(data))
            {
                var response = "HTTP/1.1 101 Switching Protocols" + Environment.NewLine
                    + "Connection: Upgrade" + Environment.NewLine
                    + "Upgrade: websocket" + Environment.NewLine
                    + "Sec-WebSocket-Accept: "
                    + Convert.ToBase64String(
                        SHA1.Create().ComputeHash(
                            Encoding.UTF8.GetBytes(
                                new Regex("Sec-WebSocket-Key: (.*)").Match(data).Groups[1].Value.Trim() + guid
                            )
                        )
                    ) + Environment.NewLine
                    + Environment.NewLine;

                //complete the handshake
                Send(response);

                var authTicketValue = new Regex(".AspNet.ApplicationCookie=(.*)").Match(data).Groups[1].Value.Trim();

                GetUserIDFromCookie(authTicketValue);

                var authedUser = UserManager.FindById(_userId);

                var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.ID.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

                if (currentCharacter == null)
                {
                    Send("<p>No character selected</p>");
                    return false;
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

                _currentPlayer.Descriptor = this;

                //We need to barf out to the connected client the welcome message. The client will only indicate connection has been established.
                var welcomeMessage = new List<String>();

                welcomeMessage.Add(string.Format("Welcome to alpha phase twinMUD, {0}", currentCharacter.FullName()));
                welcomeMessage.Add("Please feel free to LOOK around.");

                _currentPlayer.WriteTo(welcomeMessage);

                //Send the look command in
                Interpret.Render("look", _currentPlayer);
            }

            StartRead();

            return true;
        }

        private void StartRead()
        {
            var buffer = new byte[1024];
            var stream = Client.GetStream();
            stream.BeginRead(buffer, 0, 1024, new AsyncCallback(OnMessage), buffer);
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        public void OnMessage(IAsyncResult result)
        {
            if (_currentPlayer == null)
            {
                OnError(new Exception("Invalid character; please reload the client and try again."));
                return;
            }

            var stream = Client.GetStream();
            stream.EndRead(result);

            var bytes = (byte[])result.AsyncState;
            var message = Encoding.UTF8.GetString(bytes);

            var errors = Interpret.Render(message, _currentPlayer);

            //It only sends the errors
            if (errors.Any(str => !string.IsNullOrWhiteSpace(str)))
                SendWrapper(errors);

            StartRead();
        }

        public void Send(string message)
        {
            var response = Encoding.ASCII.GetBytes(message);

            var stream = Client.GetStream();

            stream.BeginWrite(response, 0, response.Length, new AsyncCallback(SendData), null);
        }

        private void SendData(IAsyncResult result)
        {
            try
            {
                var stream = Client.GetStream();

                stream.EndWrite(result);
            }
            catch { }
        }

        /// <summary>
        /// Handles when the connection closes
        /// </summary>
        public void OnClose()
        {
            Client.Close();
        }

        /// <summary>
        /// Handles when the connection faults
        /// </summary>
        /// <param name="err">the error</param>
        public void OnError(Exception err)
        {
            //Log it
            LoggingUtility.LogError(err);
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(IEnumerable<string> strings)
        {
            Send(EncapsulateOutput(strings));

            return true;
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(string str)
        {
            Send(EncapsulateOutput(str));

            return true;
        }

        public void Disconnect(string finalMessage)
        {
            Send(EncapsulateOutput(finalMessage));

            Client.Close();
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
