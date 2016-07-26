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
        }

        private async void StartLoop(Func<string, bool> worker)
        {
            NetworkStream stream = Client.GetStream();

            await DataAvailable(stream);

            var bytes = new Byte[Client.Available];

            stream.Read(bytes, 0, bytes.Length);

            //translate bytes of request to string
            var data = DecodeSocket(bytes);

            if(worker.Invoke(data))
                StartLoop(OnMessage);
            else
                OnClose();
        }

        private async Task<bool> DataAvailable(NetworkStream stream)
        {
            while (!stream.DataAvailable)
                ;

            return true;
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        public void OnOpen()
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

                //Send the handshake
                var replyBytes = Encoding.UTF8.GetBytes(response);

                stream.BeginWrite(replyBytes, 0, replyBytes.Length, new AsyncCallback(SendData), null);

                ValidateUser(data);
            }

            StartLoop(OnMessage);

            return;
        }

        private void ValidateUser(string handshake)
        {
            //Grab the user
            var authTicketValue = new Regex(".AspNet.ApplicationCookie=(.*)").Match(handshake).Groups[1].Value.Trim();

            GetUserIDFromCookie(authTicketValue);

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

            _currentPlayer.Descriptor = this;

            /*
            //We need to barf out to the connected client the welcome message. The client will only indicate connection has been established.
            var welcomeMessage = new List<String>();

            welcomeMessage.Add(string.Format("Welcome to alpha phase twinMUD, {0}", currentCharacter.FullName()));
            welcomeMessage.Add("Please feel free to LOOK around.");

            _currentPlayer.WriteTo(welcomeMessage);

            //Send the look command in
            Interpret.Render("look", _currentPlayer);
            */
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        public bool OnMessage(string message)
        {
            if (_currentPlayer == null)
            {
                OnError(new Exception("Invalid character; please reload the client and try again."));
                return false;
            }

            var errors = Interpret.Render(message, _currentPlayer);

            //It only sends the errors
            if (errors.Any(str => !string.IsNullOrWhiteSpace(str)))
                SendWrapper(errors);

            return true;
        }

        public void Send(string message)
        {
            var response = EncodeSocket(message);

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
            catch(Exception ex)
            {
                ///barf, log?
            }
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

        private string DecodeSocket(byte[] bytes)
        {
            var secondByte = bytes[1];

            var length = secondByte & 127; // may not be the actual length in the two special cases

            var indexFirstMask = 2;          // if not a special case

            if(length == 126)            // if a special case, change indexFirstMask
                indexFirstMask = 4;
            else if(length == 127)       // ditto
                indexFirstMask = 10;

            var masks = bytes.Skip(indexFirstMask).Take(4).ToArray(); // four bytes starting from indexFirstMask

            var indexFirstDataByte = indexFirstMask + 4; // four bytes further

            var decoded = new byte[bytes.Length - indexFirstDataByte]; // length of real data

            int i, j;
            for(i = indexFirstDataByte, j = 0; i < bytes.Length; i++, j++)
                decoded[j] = intToSingleByte(bytes[i] ^ masks[j % 4]);

            // now use "decoded" to interpret the received data
            return Encoding.UTF8.GetString(decoded);
        }

        private byte[] EncodeSocket(string message)
        {
            var bytesRaw = Encoding.UTF8.GetBytes(message);
            var rawLength = bytesRaw.Length;

            var formatBytes = new byte[11];
            formatBytes[0] = 129;

            int indexStartRawData;

            if (rawLength <= 125)
            {
                byte[] intBytes = BitConverter.GetBytes(rawLength);

                if (BitConverter.IsLittleEndian)
                    Array.Reverse(intBytes);

                formatBytes[1] = intToSingleByte(rawLength);

                indexStartRawData = 2;
            }
            else if(rawLength >= 126 && rawLength <= 65535)
            {
                formatBytes[1] = 126;
                formatBytes[2] = intToSingleByte((rawLength >> 8) & 255);
                formatBytes[3] = intToSingleByte(( rawLength    ) & 255);

                indexStartRawData = 4;
            }
            else
            {
                formatBytes[1] = 127;
                formatBytes[2] =  intToSingleByte(( rawLength >> 56 ) & 255);
                formatBytes[3] =  intToSingleByte(( rawLength >> 48 ) & 255);
                formatBytes[4] =  intToSingleByte(( rawLength >> 40 ) & 255);
                formatBytes[5] =  intToSingleByte(( rawLength >> 32 ) & 255);
                formatBytes[6] =  intToSingleByte(( rawLength >> 24 ) & 255);
                formatBytes[7] =  intToSingleByte(( rawLength >> 16 ) & 255);
                formatBytes[8] =  intToSingleByte(( rawLength >>  8 ) & 255);
                formatBytes[9] =  intToSingleByte(( rawLength       ) & 255);

                indexStartRawData = 10;
            }
            
            var returnBytes = new byte[rawLength + indexStartRawData + 1];

            //Add the formatted bytes first
            System.Array.Copy(formatBytes, returnBytes, indexStartRawData);

            // put raw data at the correct index
            System.Array.Copy(bytesRaw, 0, returnBytes, indexStartRawData, rawLength);

            return returnBytes;
        }

        private byte intToSingleByte(int value)
        {
            byte[] intBytes = BitConverter.GetBytes(value);

            if (BitConverter.IsLittleEndian)
                Array.Reverse(intBytes);

            return intBytes[0];
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
