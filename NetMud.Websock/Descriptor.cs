using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Authentication;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataAccess.FileSystem;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Interp;
using NetMud.Utility;
using NetMud.Websock.OutputFormatting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

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
        public ApplicationUserManager UserManager { get; private set; }

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
        public Descriptor() : this(new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext())))
        {
        }

        /// <summary>
        /// Creates an instance of the command negotiator with a specified user manager
        /// </summary>
        /// <param name="userManager">the authentication manager from the web</param>
        public Descriptor(ApplicationUserManager userManager) 
        {
            UserManager = userManager;

            Birthdate = DateTime.Now;
        }

        #region Caching
        /// <summary>
        /// What type of cache is this using
        /// </summary>
        public virtual CacheType CachingType => CacheType.Live;

        /// <summary>
        /// Put it in the cache
        /// </summary>
        /// <returns>success status</returns>
        public virtual bool PersistToCache()
        {
            try
            {
                LiveCache.Add<IDescriptor>(this);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
                return false;
            }

            return true;
        }
        #endregion

        /// <summary>
        /// Wraps sending messages to the connected descriptor
        /// </summary>
        /// <param name="strings">the output</param>
        /// <returns>success status</returns>
        public bool SendWrapper(IEnumerable<string> strings)
        {
            //TODO: Stop hardcoding this but we have literally no sense of injury/self status yet
            var self = new SelfStatus
            {
                Body = new BodyStatus
                {
                    Overall = OverallStatus.Excellent,
                    Anatomy = new AnatomicalPart[] {
                        new AnatomicalPart {
                            Name = "Arm",
                            Overall = OverallStatus.Good,
                            Wounds = new string[] {
                                "Light scrape"
                            }
                        },
                        new AnatomicalPart {
                            Name = "Leg",
                            Overall = OverallStatus.Excellent,
                            Wounds = new string[] {
                            }

                        }
                    }
                },
                Mind = new MindStatus
                {
                    Overall = OverallStatus.Excellent,
                    States = new string[]
                    {
                        "Fearful"
                    }
                }
            };

            var currentLocation = _currentPlayer.CurrentLocation;
            var currentContainer = currentLocation.CurrentLocation;
            var currentZone = currentLocation.GetZone();
            var currentWorld = currentZone.GetWorld();
            var currentRoom = currentLocation.GetRoom();

            var pathways = ((ILocation)currentContainer).GetPathways().Select(data => data.RenderAsContents(_currentPlayer, new[] { MessagingType.Visible }).ToString());
            var inventory = currentContainer.GetContents<IInanimate>().Select(data => data.RenderAsContents(_currentPlayer, new[] { MessagingType.Visible }).ToString());
            var populace = currentContainer.GetContents<IMobile>().Where(player => !player.Equals(_currentPlayer)).Select(data => data.RenderAsContents(_currentPlayer, new[] { MessagingType.Visible }).ToString());

            var local = new LocalStatus
            {
                ZoneName = currentZone.DataTemplateName,
                LocaleName = currentLocation.GetLocale()?.DataTemplateName,
                RoomName = currentRoom?.DataTemplateName,
                Inventory = inventory.ToArray(),
                Populace = populace.ToArray(),
                Exits = pathways.ToArray(),
                LocationDescriptive = currentLocation.CurrentLocation.RenderToLook(_currentPlayer).ToString()
            };

            //The next two are mostly hard coded, TODO, also fix how we get the map as that's an admin thing
            var extended = new ExtendedStatus
            {
                Horizon = new string[]
                {
                     "A hillside",
                     "A dense forest"
                },
                VisibleMap = currentLocation.GetRoom() == null ? string.Empty : currentLocation.GetRoom().RenderCenteredMap(3, true)
            };

            var timeOfDayString = string.Format("The hour of {0} in the day of {1} in {2} in the year of {3}", currentWorld.CurrentTimeOfDay.Hour
                                                                               , currentWorld.CurrentTimeOfDay.Day
                                                                               , currentWorld.CurrentTimeOfDay.MonthName()
                                                                               , currentWorld.CurrentTimeOfDay.Year);

            var visibleCelestials = Enumerable.Empty<ICelestial>();
            var visibilityString = string.Empty;

            if (currentRoom != null)
            {
                visibilityString = string.Format("{0} lumins", currentRoom.GetCurrentLuminosity());
                visibleCelestials = currentRoom.GetVisibileCelestials(_currentPlayer);
            }
            else if (currentZone != null)
            {
                visibilityString = string.Format("{0} lumins", currentZone.GetCurrentLuminosity());
                visibleCelestials = currentZone.GetVisibileCelestials(_currentPlayer);
            }
            else
                visibilityString = "I don't know";

            var celestialString = String.Join(",", visibleCelestials.Select(cp => cp.Name));

            var environment = new EnvironmentStatus
            {
                Celestial = celestialString,
                Visibility = visibilityString,
                Weather = "I don't know",
                TimeOfDay = timeOfDayString
            };

            var outputFormat = new OutputStatus
            {
                Occurrence = EncapsulateOutput(strings),
                Self = self,
                Local = local,
                Extended = extended,
                Environment = environment
            };

            Send(SerializationUtility.Serialize(outputFormat));

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

            Close();
        }

        /// <summary>
        /// Open the socket, the client gets set up in the constructor
        /// </summary>
        public void Open()
        {
            new Task(OnOpen).Start();
        }

        #region "Socket Management"
        public override void OnClose()
        {
            var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null
                        && player.DataTemplate<ICharacter>().Account.Config.WantsNotification(_currentPlayer.AccountHandle, false, AcquaintenceNotifications.LeaveGame));

            foreach (var player in validPlayers)
                player.WriteTo(new string[] { string.Format("{0} has left the game.", _currentPlayer.AccountHandle) });

            if (_currentPlayer.DataTemplate<ICharacter>().Account.Config.GossipSubscriber)
            {
                var gossipClient = LiveCache.Get<IGossipClient>("GossipWebClient");

                if (gossipClient != null)
                    gossipClient.SendNotification(_currentPlayer.AccountHandle, AcquaintenceNotifications.LeaveGame);
            }

            base.OnClose();
        }

        /// <summary>
        /// Handles initial connection
        /// </summary>
        public override void OnOpen()
        {
            base.OnOpen();

            BirthMark = LiveCache.GetUniqueIdentifier(string.Format(cacheKeyFormat, WebSocketContext.AnonymousID));
            PersistToCache();

            UserManager = new ApplicationUserManager(new UserStore<ApplicationUser>(new ApplicationDbContext()));

            ValidateUser(WebSocketContext.CookieCollection[".AspNet.ApplicationCookie"]);

            LoggingUtility.Log(content: "Socket client accepted", channel: LogChannels.SocketCommunication);
        }

        /// <summary>
        /// Handles when the connected descriptor sends input
        /// </summary>
        /// <param name="e">the events of the message</param>
        public override void OnMessage(string message)
        {
            if (_currentPlayer == null)
            {
                OnError(new Exception("Invalid character; please reload the client and try again."));
                return;
            }

            var errors = Interpret.Render(message, _currentPlayer);

            //It only sends the errors
            if (errors.Any(str => !string.IsNullOrWhiteSpace(str)))
                SendWrapper(errors);
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
        /// Ping the client for keepalive
        /// </summary>
        private void SendPing()
        {
            var ping = new byte[2];

            ping[0] = 9 | 0x80;
            ping[1] = 0;

            Send(ping);
        }
        #endregion

        #region "Helpers"
        /// <summary>
        /// Validates the game account from the aspnet cookie
        /// </summary>
        /// <param name="handshake">the headers from the http request</param>
        private void ValidateUser(Cookie cookie)
        {
            //Grab the user
            GetUserIDFromCookie(cookie.Value);

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
                string.Format("Welcome to alpha phase Under the Eclipse, {0}", currentCharacter.FullName()),
                "Please feel free to LOOK around."
            };

            _currentPlayer.WriteTo(welcomeMessage);

            //Send the look command in
            Interpret.Render("look", _currentPlayer);

            try
            {
                var validPlayers = LiveCache.GetAll<IPlayer>().Where(player => player.Descriptor != null
                && player.DataTemplate<ICharacter>().Account.Config.WantsNotification(_currentPlayer.AccountHandle, false, AcquaintenceNotifications.EnterGame));

                foreach (var player in validPlayers)
                    player.WriteTo(new string[] { string.Format("{0} has entered the game.", _currentPlayer.AccountHandle) });

                if (authedUser.GameAccount.Config.GossipSubscriber)
                {
                    var gossipClient = LiveCache.Get<IGossipClient>("GossipWebClient");

                    if (gossipClient != null)
                        gossipClient.SendNotification(authedUser.GlobalIdentityHandle, DataStructure.Base.PlayerConfiguration.AcquaintenceNotifications.EnterGame);
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SocketCommunication);
            }
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

        #region Equality Functions
        /// <summary>
        /// -99 = null input
        /// -1 = wrong type
        /// 0 = same type, wrong id
        /// 1 = same reference (same id, same type)
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(ILiveData other)
        {
            if (other != null)
            {
                try
                {
                    if (other.GetType() != GetType())
                        return -1;

                    if (other.BirthMark.Equals(BirthMark))
                        return 1;

                    return 0;
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return -99;
        }

        /// <summary>
        /// Compares this object to another one to see if they are the same object
        /// </summary>
        /// <param name="other">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData other)
        {
            if (other != default(ILiveData))
            {
                try
                {
                    return other.GetType() == GetType() && other.BirthMark.Equals(BirthMark);
                }
                catch (Exception ex)
                {
                    LoggingUtility.LogError(ex);
                }
            }

            return false;
        }

        /// <summary>
        /// Compares an object to another one to see if they are the same object
        /// </summary>
        /// <param name="x">the object to compare to</param>
        /// <param name="y">the object to compare to</param>
        /// <returns>true if the same object</returns>
        public bool Equals(ILiveData x, ILiveData y)
        {
            return x.Equals(y);
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <param name="obj">the thing to get the hashcode for</param>
        /// <returns>the hash code</returns>
        public int GetHashCode(ILiveData obj)
        {
            return obj.GetType().GetHashCode() + obj.BirthMark.GetHashCode();
        }

        /// <summary>
        /// Get the hash code for comparison purposes
        /// </summary>
        /// <returns>the hash code</returns>
        public override int GetHashCode()
        {
            return GetType().GetHashCode() + BirthMark.GetHashCode();
        }
        #endregion
    }
}
