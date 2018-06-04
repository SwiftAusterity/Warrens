using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.System;
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
        public ApplicationUserManager UserManager { get; private set; }

        /// <summary>
        /// The actual connection's client handler
        /// </summary>
        internal TcpClient Client { get; set; }

        /// <summary>
        /// Unique string for this live entity
        /// </summary>
        public string BirthMark { get; set; }

        /// <summary>
        /// When this entity was born to the world
        /// </summary>
        public DateTime Birthdate { get; set; }

        /// <summary>
        /// User id of connected player
        /// </summary>
        private string _userId;

        /// <summary>
        /// The player connected
        /// </summary>
        private IPlayer _currentPlayer;

        /// <summary>
        /// Creates an instance of the command negotiator
        /// </summary>
        public Descriptor(TcpClient tcpClient)
        {
            Client = tcpClient;
            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            BirthMark = LiveCache.GetUniqueIdentifier(string.Format(cacheKeyFormat, Client.Client.RemoteEndPoint.Serialize().ToString()));
            Birthdate = DateTime.Now;

            LiveCache.Add<IDescriptor>(this);
        }

        /// <summary>
        /// Creates an instance of the command negotiator with a specified user manager
        /// </summary>
        /// <param name="userManager">the authentication manager from the web</param>
        public Descriptor(TcpClient tcpClient, ApplicationUserManager userManager)
        {
            Client = tcpClient;
            UserManager = userManager;

            BirthMark = LiveCache.GetUniqueIdentifier(string.Format(cacheKeyFormat, Client.Client.RemoteEndPoint.Serialize().ToString()));
            Birthdate = DateTime.Now;

            LiveCache.Add<IDescriptor>(this);
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(IEnumerable<string> strings)
        {
            //TODO: Add "robust output format" here
            var output = EncapsulateOutput(strings);

            Send(output);

            return true;
        }

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="str">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(string str)
        {
            //Easier just to handle it in one place
            return SendWrapper(new List<string>() { str });
        }

        /// <summary>
        /// Disconnects the client socket
        /// </summary>
        /// <param name="finalMessage">the final string data to send the socket before closing it</param>
        public void Disconnect(string finalMessage)
        {
            SendWrapper(finalMessage);

            Client.Close();
        }

        /// <summary>
        /// Open the socket, the client gets set up in the constructor
        /// </summary>
        public void Open()
        {
            new Task(OnOpen).Start();
        }

        #region "Socket Management"

        /// <summary>
        /// Handles initial connection
        /// </summary>
        private async void OnOpen()
        {
            NetworkStream stream = Client.GetStream();

            //enter to an infinite cycle to be able to handle every change in stream
            await DataAvailable(stream);

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

                stream.BeginWrite(replyBytes, 0, replyBytes.Length, new AsyncCallback(WriteData), null);

                ValidateUser(data);
            }

            StartLoop(OnMessage);

            return;
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        private bool OnMessage(string message)
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

        /// <summary>
        /// Handles when the connection faults
        /// </summary>
        /// <param name="err">the error</param>
        private void OnError(Exception ex)
        {
            //Log it
            LoggingUtility.LogError(ex, false);
        }

        /// <summary>
        /// Begins the send process for putting data into the socket stream
        /// </summary>
        /// <param name="message">the string message</param>
        private void Send(string message)
        {
            var response = EncodeSocket(message);

            var stream = Client.GetStream();

            stream.BeginWrite(response, 0, response.Length, new AsyncCallback(WriteData), null);
        }

        /// <summary>
        /// Ping the client for keepalive
        /// </summary>
        private void SendPing()
        {
            var stream = Client.GetStream();

            var ping = new byte[2];

            ping[0] = 9 | 0x80;
            ping[1] = 0;

            stream.BeginWrite(ping, 0, 2, new AsyncCallback(WriteData), null);
        }

        /// <summary>
        /// Ends the send loop
        /// </summary>
        /// <param name="result">the async object for the thread</param>
        private void WriteData(IAsyncResult result)
        {
            try
            {
                var stream = Client.GetStream();

                stream.EndWrite(result);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }
        #endregion

        #region "Helpers"

        /// <summary>
        /// Handles when the connection closes
        /// </summary>
        private void OnClose()
        {
            Client.Close();
        }

        /// <summary>
        /// Handles the wait loop for accepting input from the socket
        /// </summary>
        /// <param name="worker">the function that actually takes in a full message from the socker</param>
        private async void StartLoop(Func<string, bool> worker)
        {
            if (Client == null)
                OnClose();

            try
            {
                NetworkStream stream = Client.GetStream();

                await DataAvailable(stream);

                var bytes = new Byte[Client.Available];

                stream.Read(bytes, 0, bytes.Length);

                //translate bytes of request to string
                var data = DecodeSocket(bytes);

                if (!string.IsNullOrWhiteSpace(data))
                {
                    if (worker.Invoke(data))
                        StartLoop(OnMessage);
                    else
                        OnClose();
                }
                else
                    StartLoop(OnMessage);
            }
            catch (Exception ex)
            {
                OnError(ex);
            }
        }

        /// <summary>
        /// Just sits on the stream waiting for a messgae to be sent
        /// </summary>
        /// <param name="stream">the network stream of the client</param>
        /// <returns>junk boolean cause it's a task</returns>
        private async Task<bool> DataAvailable(NetworkStream stream)
        {
            var timeIdle = 0;
            while (!stream.DataAvailable)
            {
                if (Client == null)
                {
                    OnClose();
                    return false;
                }

                await Task.Delay(500);

                switch (timeIdle)
                {
                    default:
                        {
                            if (timeIdle > 0 && timeIdle % 15 == 0)
                                SendPing();

                            break;
                        }
                    case 600:
                        {
                            Send("You have been idle for an extended period of time. You will be logged out shortly.");
                            break;
                        }
                    case 1200:
                        {
                            Disconnect("You have been idle too long. You have been disconnected.");
                            return false;
                        }
                }

                timeIdle++;
            }

            return true;
        }


        /// <summary>
        /// Validates the game account from the aspnet cookie
        /// </summary>
        /// <param name="handshake">the headers from the http request</param>
        private void ValidateUser(string handshake)
        {
            //Grab the user
            var authTicketValue = new Regex(".AspNet.ApplicationCookie=(.*)").Match(handshake).Groups[1].Value.Trim();

            GetUserIDFromCookie(authTicketValue);

            var authedUser = UserManager.FindById(_userId);

            var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.Id.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

            if (currentCharacter == null)
            {
                Send("<p>No character selected</p>");
                return;
            }

            //Try to see if they are already live
            _currentPlayer = LiveCache.Get<IPlayer>(currentCharacter.Id);

            //Check the backup
            if (_currentPlayer == null)
            {
                var playerDataWrapper = new PlayerData();
                _currentPlayer = playerDataWrapper.RestorePlayer(currentCharacter.AccountHandle, currentCharacter.Id);
            }

            //else new them up
            if (_currentPlayer == null)
                _currentPlayer = new Player(currentCharacter);

            _currentPlayer.Descriptor = this;

            //We need to barf out to the connected client the welcome message. The client will only indicate connection has been established.
            var welcomeMessage = new List<string>
            {
                string.Format("Welcome to alpha phase twinMUD, {0}", currentCharacter.FullName()),
                "Please feel free to LOOK around."
            };

            _currentPlayer.WriteTo(welcomeMessage);

            //Send the look command in
            Interpret.Render("look", _currentPlayer);
        }

        /// <summary>
        /// Decodes WS headers and data from the stream
        /// </summary>
        /// <param name="buffer">the stream's incoming data</param>
        /// <returns>the message sent</returns>
        private string DecodeSocket(byte[] buffer)
        {
            var length = buffer.Length;
            byte b = buffer[1];
            int dataLength = 0;
            int totalLength = 0;
            int keyIndex = 0;

            if (b - 128 <= 125)
            {
                dataLength = b - 128;
                keyIndex = 2;
                totalLength = dataLength + 6;
            }

            if (b - 128 == 126)
            {
                dataLength = BitConverter.ToInt16(new byte[] { buffer[3], buffer[2] }, 0);
                keyIndex = 4;
                totalLength = dataLength + 8;
            }

            if (b - 128 == 127)
            {
                dataLength = (int)BitConverter.ToInt64(new byte[] { buffer[9], buffer[8], buffer[7], buffer[6], buffer[5], buffer[4], buffer[3], buffer[2] }, 0);
                keyIndex = 10;
                totalLength = dataLength + 14;
            }

            if (totalLength > length)
                throw new Exception("The buffer length is small than the data length");

            byte[] key = new byte[] { buffer[keyIndex], buffer[keyIndex + 1], buffer[keyIndex + 2], buffer[keyIndex + 3] };

            int dataIndex = keyIndex + 4;
            int count = 0;
            for (int i = dataIndex; i < totalLength; i++)
            {
                buffer[i] = (byte)(buffer[i] ^ key[count % 4]);
                count++;
            }

            return Encoding.ASCII.GetString(buffer, dataIndex, dataLength);
        }

        /// <summary>
        /// Encodes string messages into ws socket language
        /// </summary>
        /// <param name="message">the data to encode</param>
        /// <returns>the data to put on the stream</returns>
        private Byte[] EncodeSocket(string message)
        {
            Byte[] response;
            Byte[] bytesRaw = Encoding.UTF8.GetBytes(message);
            Byte[] frame = new Byte[10];

            Int32 indexStartRawData = -1;
            Int32 length = bytesRaw.Length;

            frame[0] = 129;
            if (length <= 125)
            {
                frame[1] = (Byte)length;
                indexStartRawData = 2;
            }
            else if (length >= 126 && length <= 65535)
            {
                frame[1] = 126;
                frame[2] = (Byte)((length >> 8) & 255);
                frame[3] = (Byte)(length & 255);
                indexStartRawData = 4;
            }
            else
            {
                frame[1] = 127;
                frame[2] = (Byte)((length >> 56) & 255);
                frame[3] = (Byte)((length >> 48) & 255);
                frame[4] = (Byte)((length >> 40) & 255);
                frame[5] = (Byte)((length >> 32) & 255);
                frame[6] = (Byte)((length >> 24) & 255);
                frame[7] = (Byte)((length >> 16) & 255);
                frame[8] = (Byte)((length >> 8) & 255);
                frame[9] = (Byte)(length & 255);

                indexStartRawData = 10;
            }

            response = new Byte[indexStartRawData + length];

            Int32 i, reponseIdx = 0;

            //Add the frame bytes to the reponse
            for (i = 0; i < indexStartRawData; i++)
            {
                response[reponseIdx] = frame[i];
                reponseIdx++;
            }

            //Add the data bytes to the response
            for (i = 0; i < length; i++)
            {
                response[reponseIdx] = bytesRaw[i];
                reponseIdx++;
            }

            return response;
        }

        /// <summary>
        /// Gets the user Id from the web from the aspnet cookie
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
        #endregion
    }
}
