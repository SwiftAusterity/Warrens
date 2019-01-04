using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Player;
using NetMud.Models.Admin;
using NetMud.Models.PlayerManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;

        public ManageController()
        {
        }

        public ManageController(ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            UserManager = userManager;
            SignInManager = signInManager;
        }

        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set
            {
                _signInManager = value;
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        [HttpGet]
        public ActionResult Index()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            Account account = user.GameAccount;

            IPlayerTemplate obj = account.Character;
            ManageAccountViewModel model = new ManageAccountViewModel
            {
                authedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle,
                UITutorialMode = account.Config.UITutorialMode,
                GossipSubscriber = account.Config.GossipSubscriber,
                PermanentlyMuteMusic = account.Config.MusicMuted,
                PermanentlyMuteSound = account.Config.SoundMuted,
                CharacterObject = obj,
                Name = obj.Name,
                SurName = obj.SurName,
                Gender = obj.Gender,
                SuperVision = obj.SuperVision,
                AsciiCharacter = obj.AsciiCharacter,
                HexColorCode = obj.HexColorCode,
                ChosenRole = user.GetStaffRank(User),
                ValidRoles = (StaffRank[])Enum.GetValues(typeof(StaffRank))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccountConfig(ManageAccountViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            Account obj = authedUser.GameAccount;

            obj.Config.UITutorialMode = vModel.UITutorialMode;
            obj.Config.GossipSubscriber = vModel.GossipSubscriber;
            obj.Config.MusicMuted = vModel.PermanentlyMuteMusic;
            obj.Config.SoundMuted = vModel.PermanentlyMuteSound;

            if (vModel.LogChannels != null)
                obj.LogChannelSubscriptions = vModel.LogChannels;

            UserManager.UpdateAsync(authedUser);

            if (obj.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.Log("*WEB* - EditGameAccount[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                message = "Edit Successful.";
            }
            else
                message = "Error; edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        #region Playlists
        [HttpGet]
        public ActionResult Playlists(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            HashSet<IPlaylist> lists = authedUser.GameAccount.Config.Playlists;

            ManagePlaylistsViewModel model = new ManagePlaylistsViewModel(lists)
            {
                authedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddPlaylist()
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            AddEditPlaylistViewModel vModel = new AddEditPlaylistViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidSongs = ContentUtility.GetMusicTracksForZone(authedUser.GameAccount.Character.CurrentLocation.CurrentZone)
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPlaylist(AddEditPlaylistViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;

            if (existingPlaylists.Any(list => list.Name.Equals(vModel.Name)))
                message = "A playlist by that name already exists.";
            else if (vModel.SongList == null || vModel.SongList.Length == 0)
                message = "Your playlist needs at least one song in it.";
            else
            {
                Playlist playlist = new Playlist()
                {
                    Name = vModel.Name,
                    Songs = new HashSet<string>(vModel.SongList)
                };

                authedUser.GameAccount.Config.Playlists.Add(playlist);

                if (!authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.Log("*WEB* - AddPlaylist[" + vModel.Name + "]", LogChannels.AccountActivity);
                }
            }

            return RedirectToAction("Playlists", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditPlaylist(string name)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;
            IPlaylist obj = existingPlaylists.FirstOrDefault(list => list.Name.Equals(name));

            if (obj == null)
            {
                return RedirectToAction("Playlists", new { Message = "That playlist does not exist." });
            }

            AddEditPlaylistViewModel vModel = new AddEditPlaylistViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                Name = obj.Name,
                DataObject = obj,
                ValidSongs = ContentUtility.GetMusicTracksForZone(authedUser.GameAccount.Character.CurrentLocation.CurrentZone)
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPlaylist(AddEditPlaylistViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            HashSet<IPlaylist> existingPlaylists = authedUser.GameAccount.Config.Playlists;
            IPlaylist obj = existingPlaylists.FirstOrDefault(list => list.Name.Equals(vModel.Name));

            if (obj == null)
            {
                return RedirectToAction("Playlists", new { Message = "That playlist does not exist." });
            }

            authedUser.GameAccount.Config.Playlists.Remove(obj);

            Playlist playlist = new Playlist()
            {
                Name = vModel.Name,
                Songs = new HashSet<string>(vModel.SongList)
            };

            authedUser.GameAccount.Config.Playlists.Add(playlist);

            if (!authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                message = "Error; Edit failed.";
            else
            {
                LoggingUtility.Log("*WEB* - EditPlaylist[" + vModel.Name + "]", LogChannels.AccountActivity);
            }

            return RedirectToAction("Playlists", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/RemovePlaylist/{ID?}/{authorize?}")]
        public ActionResult RemovePlaylist(string removePlaylistName = "", string authorizeRemovePlaylist = "")
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorizeRemovePlaylist))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
                string[] values = authorizeRemovePlaylist.Split(new string[] { "|||" }, StringSplitOptions.RemoveEmptyEntries);

                if (values.Count() != 2)
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    IPlaylist origin = authedUser.GameAccount.Config.Playlists.FirstOrDefault(list => list.Name.Equals(removePlaylistName));

                    if (origin == null)
                        message = "That playlist does not exist";
                    else
                    {
                        authedUser.GameAccount.Config.Playlists = new HashSet<IPlaylist>(authedUser.GameAccount.Config.Playlists.Where(list => !list.Name.Equals(removePlaylistName)));

                        if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                        {
                            LoggingUtility.Log("*WEB* - RemoveZonePath[" + removePlaylistName + "]", LogChannels.AccountActivity);
                            message = "Delete Successful.";
                        }
                        else
                            message = "Error; Removal failed.";
                    }
                }
            }

            return RedirectToAction("Playlists", new { Message = message });
        }
        #endregion

        #region Characters
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCharacter(long id, ManageAccountViewModel vModel)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);
            IPlayerTemplate obj = PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), authedUser.GlobalIdentityHandle, id));

            if (obj == null)
                message = "That character does not exist";
            else
            {
                obj.SuperVision = vModel.SuperVision;
                obj.Name = vModel.Name;
                obj.SurName = vModel.SurName;
                obj.Gender = vModel.Gender;
                obj.AsciiCharacter = vModel.AsciiCharacter;
                obj.HexColorCode = vModel.HexColorCode;

                if (User.IsInRole("Admin"))
                    obj.GamePermissionsRank = vModel.ChosenRole;
                else
                    obj.GamePermissionsRank = StaffRank.Player;

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.Log("*WEB* - EditCharacter[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Notifications
        [HttpGet]
        public ActionResult Notifications(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            IEnumerable<IPlayerMessage> notifications = authedUser.GameAccount.Config.Notifications;

            ManageNotificationsViewModel model = new ManageNotificationsViewModel(notifications)
            {
                authedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddViewNotification(string id)
        {
            string userId = User.Identity.GetUserId();
            AddViewNotificationViewModel model = new AddViewNotificationViewModel
            {
                authedUser = UserManager.FindById(userId)
            };

            if (!string.IsNullOrWhiteSpace(id))
            {
                var message = ConfigDataCache.Get<IPlayerMessage>(id);

                if (message != null)
                {
                    model.DataObject = message;
                    model.Body = message.Body;
                    model.Recipient = message.RecipientName;
                    model.Subject = message.Subject;
                }
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddViewNotification(AddViewNotificationViewModel vModel)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            try
            {
                if (string.IsNullOrWhiteSpace(vModel.Body) || string.IsNullOrWhiteSpace(vModel.Subject))
                    message = "You must include a valid body and subject.";
                else
                {
                    if (string.IsNullOrWhiteSpace(vModel.RecipientAccount))
                        message = "You must include a valid recipient.";
                    else
                    {
                        IAccount recipient = Account.GetByHandle(vModel.RecipientAccount);

                        if (recipient == null || recipient.Config.Acquaintences.Any(acq => acq.IsFriend == false && acq.PersonHandle.Equals(authedUser.GameAccount.GlobalIdentityHandle)))
                            message = "You must include a valid recipient.";
                        else
                        {
                            PlayerMessage newMessage = new PlayerMessage
                            {
                                Body = vModel.Body,
                                Subject = vModel.Subject,
                                Sender = authedUser.GameAccount,
                                RecipientAccount = recipient
                            };

                            var recipientCharacter = TemplateCache.GetByName<IPlayerTemplate>(vModel.Recipient);

                            if (recipientCharacter != null)
                                newMessage.Recipient = recipientCharacter;

                            //messages come from players always here
                            if (newMessage.Save(authedUser.GameAccount, StaffRank.Player))
                            {
                                message = "Successfully sent.";
                            }
                            else
                            {
                                LoggingUtility.Log("Message unsuccessful.", LogChannels.SystemWarnings);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
            }

            return RedirectToAction("Notifications", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsReadNotification(string id, AddViewNotificationViewModel vModel)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            try
            {
                if (!string.IsNullOrWhiteSpace(id))
                {
                    var notification = ConfigDataCache.Get<IPlayerMessage>(id);

                    if (notification != null)
                    {
                        notification.Read = true;
                        notification.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }
                else
                    message = "Invalid message.";
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, LogChannels.SystemWarnings);
            }

            return RedirectToAction("Notifications", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveNotification(string ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IPlayerMessage notification = authedUser.GameAccount.Config.Notifications.FirstOrDefault(ch => ch.UniqueKey.Equals(ID));

                if (notification == null)
                    message = "That message does not exist";
                else if (notification.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    message = "Message successfully deleted.";
                else
                    message = "Error. Message not removed.";
            }

            return RedirectToAction("Notifications", new { Message = message });
        }
        #endregion

        #region Acquaintences
        [HttpGet]
        public ActionResult Acquaintences(string message = "")
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            IEnumerable<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences;

            ManageAcquaintencesViewModel model = new ManageAcquaintencesViewModel(acquaintences)
            {
                authedUser = authedUser
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddAcquaintence(string AcquaintenceName, bool IsFriend, bool GossipSystem, string Notifications)
        {
            string message = string.Empty;
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);

            if (AcquaintenceName.Equals(authedUser.GlobalIdentityHandle, StringComparison.InvariantCultureIgnoreCase))
            {
                message = "You can't become an acquaintence of yourself.";
            }
            else
            {
                List<AcquaintenceNotifications> notificationsList = new List<AcquaintenceNotifications>();

                foreach (string notification in Notifications.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var anShort = (AcquaintenceNotifications)Enum.Parse(typeof(AcquaintenceNotifications), notification);

                    notificationsList.Add(anShort);
                }

                Acquaintence newAcq = new Acquaintence
                {
                    PersonHandle = AcquaintenceName,
                    IsFriend = IsFriend,
                    GossipSystem = GossipSystem,
                    NotificationSubscriptions = notificationsList.ToArray()
                };

                List<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences.ToList();

                if (acquaintences.Contains(newAcq))
                    acquaintences.Remove(newAcq);

                acquaintences.Add(newAcq);
                authedUser.GameAccount.Config.Acquaintences = acquaintences;

                if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    message = "Acquaintence successfully added.";
                else
                    message = "Error. Acquaintence not added.";
            }

            return RedirectToAction("Acquaintences", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/RemoveAcquaintence/{ID?}/{authorize?}")]
        public ActionResult RemoveAcquaintence(string ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IAcquaintence acquaintence = authedUser.GameAccount.Config.Acquaintences.FirstOrDefault(ch => ch.PersonHandle.Equals(ID));

                if (acquaintence == null)
                    message = "That Acquaintence does not exist";
                else
                {
                    List<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences.ToList();

                    acquaintences.Remove(acquaintence);

                    if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                        message = "Acquaintence successfully deleted.";
                    else
                        message = "Error. Acquaintence not removed.";
                }
            }

            return RedirectToAction("Acquaintences", new { Message = message });
        }
        #endregion

        #region AuthStuff
        [HttpGet]
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            IdentityResult result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index");
            }
            AddErrors(result);
            return View(model);
        }

        [HttpGet]
        public ActionResult SetPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityResult result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    ApplicationUser user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                    if (user != null)
                    {
                        await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }
                    return RedirectToAction("Index");
                }
                AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }
        #endregion

        #region Helpers
        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (string error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        #endregion
    }
}