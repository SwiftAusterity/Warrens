using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Authentication;
using NetMud.Data.Combat;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Combat;
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

            ManageAccountViewModel model = new ManageAccountViewModel
            {
                AuthedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle,
                ComboCount = account.Config.Combos.Count(),
                FightingArtCount = TemplateCache.GetAll<IFightingArt>().Count(art => art.CreatorHandle == user.GlobalIdentityHandle),
                NotificationCount = ConfigDataCache.GetAll<IPlayerMessage>().Count(msg => msg.RecipientAccount == account),
                UITutorialMode = account.Config.UITutorialMode,
                GossipSubscriber = account.Config.GossipSubscriber,
                PermanentlyMuteMusic = account.Config.MusicMuted,
                PermanentlyMuteSound = account.Config.SoundMuted,
                ChosenRole = user.GetStaffRank(User),
                ValidRoles = (StaffRank[])Enum.GetValues(typeof(StaffRank))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccountConfig(ManageAccountViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            Account obj = authedUser.GameAccount;

            obj.Config.UITutorialMode = vModel.UITutorialMode;
            obj.Config.GossipSubscriber = vModel.GossipSubscriber;
            obj.Config.MusicMuted = vModel.PermanentlyMuteMusic;
            obj.Config.SoundMuted = vModel.PermanentlyMuteSound;

            if (vModel.LogChannels != null)
            {
                obj.LogChannelSubscriptions = vModel.LogChannels;
            }

            UserManager.UpdateAsync(authedUser);
            string message;
            if (obj.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.Log("*WEB* - EditGameAccount[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        #region Characters
        [HttpGet]
        public ActionResult ManageCharacters(string message)
        {
            ViewBag.StatusMessage = message;

            string userId = User.Identity.GetUserId();
            ManageCharactersViewModel model = new ManageCharactersViewModel
            {
                AuthedUser = UserManager.FindById(userId),
                NewCharacter = new PlayerTemplate()
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCharacter(ManageCharactersViewModel vModel)
        {
            string userId = User.Identity.GetUserId();
            ManageCharactersViewModel model = new ManageCharactersViewModel
            {
                AuthedUser = UserManager.FindById(userId)
            };

            PlayerTemplate newChar = new PlayerTemplate
            {
                Name = vModel.NewCharacter.Name,
                SurName = vModel.NewCharacter.SurName,
                GamePermissionsRank = StaffRank.Player,
            };

            if (User.IsInRole("Admin"))
            {
                newChar.GamePermissionsRank = vModel.NewCharacter.GamePermissionsRank;
            }

            string message = model.AuthedUser.GameAccount.AddCharacter(newChar);

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditCharacter(long id)
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser user = UserManager.FindById(userId);

            IPlayerTemplate obj = PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), user.GlobalIdentityHandle, id));
            AddEditCharacterViewModel model = new AddEditCharacterViewModel
            {
                AuthedUser = user,
                DataObject = obj
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCharacter(long id, AddEditCharacterViewModel vModel)
        {
            string userId = User.Identity.GetUserId();
            ApplicationUser authedUser = UserManager.FindById(userId);
            IPlayerTemplate obj = PlayerDataCache.Get(new PlayerDataCacheKey(typeof(IPlayerTemplate), authedUser.GlobalIdentityHandle, id));

            obj.Name = vModel.DataObject.Name;
            obj.SurName = vModel.DataObject.SurName;
            obj.GamePermissionsRank = vModel.DataObject.GamePermissionsRank;
            string message;
            if (obj == null)
            {
                message = "That character does not exist";
            }
            else
            {
                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.Log("*WEB* - EditCharacter[" + authedUser.GameAccount.GlobalIdentityHandle + "]", LogChannels.AccountActivity);
                    message = "Edit Successful.";
                }
                else
                {
                    message = "Error; edit failed.";
                }
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCharacter(long removeId, string authorizeRemove)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorizeRemove) || !removeId.ToString().Equals(authorizeRemove))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ManageCharactersViewModel model = new ManageCharactersViewModel
                {
                    AuthedUser = UserManager.FindById(userId)
                };

                IPlayerTemplate character = model.AuthedUser.GameAccount.Characters.FirstOrDefault(ch => ch.Id.Equals(removeId));

                if (character == null)
                {
                    message = "That character does not exist";
                }
                else if (character.Remove(model.AuthedUser.GameAccount, model.AuthedUser.GetStaffRank(User)))
                {
                    message = "Character successfully deleted.";
                }
                else
                {
                    message = "Error. Character not removed.";
                }
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
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
                AuthedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddViewNotification(string id)
        {
            string userId = User.Identity.GetUserId();
            AddViewNotificationViewModel model = new AddViewNotificationViewModel
            {
                AuthedUser = UserManager.FindById(userId)
            };

            if (!string.IsNullOrWhiteSpace(id))
            {
                IPlayerMessage message = ConfigDataCache.Get<IPlayerMessage>(id);

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
                {
                    message = "You must include a valid body and subject.";
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(vModel.RecipientAccount))
                    {
                        message = "You must include a valid recipient.";
                    }
                    else
                    {
                        IAccount recipient = Account.GetByHandle(vModel.RecipientAccount);

                        if (recipient == null || recipient.Config.Acquaintences.Any(acq => acq.IsFriend == false && acq.PersonHandle.Equals(authedUser.GameAccount.GlobalIdentityHandle)))
                        {
                            message = "You must include a valid recipient.";
                        }
                        else
                        {
                            PlayerMessage newMessage = new PlayerMessage
                            {
                                Body = vModel.Body,
                                Subject = vModel.Subject,
                                Sender = authedUser.GameAccount,
                                RecipientAccount = recipient
                            };

                            IPlayerTemplate recipientCharacter = TemplateCache.GetByName<IPlayerTemplate>(vModel.Recipient);

                            if (recipientCharacter != null)
                            {
                                newMessage.Recipient = recipientCharacter;
                            }

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
                    IPlayerMessage notification = ConfigDataCache.Get<IPlayerMessage>(id);

                    if (notification != null)
                    {
                        notification.Read = true;
                        notification.Save(authedUser.GameAccount, authedUser.GetStaffRank(User));
                    }
                }
                else
                {
                    message = "Invalid message.";
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
        public ActionResult RemoveNotification(string ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IPlayerMessage notification = authedUser.GameAccount.Config.Notifications.FirstOrDefault(ch => ch.UniqueKey.Equals(ID));

                if (notification == null)
                {
                    message = "That message does not exist";
                }
                else if (notification.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    message = "Message successfully deleted.";
                }
                else
                {
                    message = "Error. Message not removed.";
                }
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
                AuthedUser = authedUser
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
                    AcquaintenceNotifications anShort = (AcquaintenceNotifications)Enum.Parse(typeof(AcquaintenceNotifications), notification);

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

                if (acquaintences.Any(aq => aq.PersonHandle == newAcq.PersonHandle))
                {
                    acquaintences.Remove(newAcq);
                }

                acquaintences.Add(newAcq);
                authedUser.GameAccount.Config.Acquaintences = acquaintences;

                if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    message = "Acquaintence successfully added.";
                }
                else
                {
                    message = "Error. Acquaintence not added.";
                }
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
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {

                string userId = User.Identity.GetUserId();
                ApplicationUser authedUser = UserManager.FindById(userId);

                IAcquaintence acquaintence = authedUser.GameAccount.Config.Acquaintences.FirstOrDefault(ch => ch.PersonHandle.Equals(ID));

                if (acquaintence == null)
                {
                    message = "That Acquaintence does not exist";
                }
                else
                {
                    List<IAcquaintence> acquaintences = authedUser.GameAccount.Config.Acquaintences.ToList();

                    acquaintences.Remove(acquaintence);

                    if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        message = "Acquaintence successfully deleted.";
                    }
                    else
                    {
                        message = "Error. Acquaintence not removed.";
                    }
                }
            }

            return RedirectToAction("Acquaintences", new { Message = message });
        }
        #endregion

        #region Fighting Arts
        public ActionResult FightingArts(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            ManageFightingArtViewModel vModel = new ManageFightingArtViewModel(TemplateCache.GetAll<IFightingArt>().Where(art => art.CreatorHandle == authedUser.GlobalIdentityHandle))
            {
                AuthedUser = authedUser,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/Manage/FightingArts.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Manage/FightingArtRemove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult FightingArtRemove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IFightingArt obj = TemplateCache.Get<IFightingArt>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IFightingArt obj = TemplateCache.Get<IFightingArt>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRoom[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                {
                    message = "Error; Unapproval failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult FightingArtAdd()
        {
            AddEditFightingArtViewModel vModel = new AddEditFightingArtViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new FightingArt()
            };

            return View("~/Views/Manage/FightingArtAdd.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FightingArtAdd(AddEditFightingArtViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IFightingArt newObj = vModel.DataObject;

            if(newObj.CalculateCostRatio() > 0)
            {
                ViewData.Add("Message", "The Calculated Cost must be equal to or below zero.");
                return View("~/Views/Manage/FightingArtAdd.cshtml", vModel);
            }

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddFightingArt[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("FightingArts", new { Message = message });
        }

        [HttpGet]
        public ActionResult FightingArtEdit(int id)
        {
            string message = string.Empty;
            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditFightingArtViewModel vModel = new AddEditFightingArtViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj,
            };

            return View("~/Views/Manage/FightingArtEdit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult FightingArtEdit(int id, AddEditFightingArtViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);
            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("Index", new { StatusMessage = message });
            }

            if (vModel.DataObject.CalculateCostRatio() > 0)
            {
                ViewData.Add("Message", "The Calculated Cost must be equal to or below zero.");
                return View("~/Views/Manage/FightingArtEdit.cshtml", vModel);
            }

            obj.Name = vModel.DataObject.Name;
            obj.ActorCriteria = vModel.DataObject.ActorCriteria;
            obj.Aim = vModel.DataObject.Aim;
            obj.Armor = vModel.DataObject.Armor;
            obj.DistanceChange = vModel.DataObject.DistanceChange;
            obj.DistanceRange = vModel.DataObject.DistanceRange;
            obj.Health = vModel.DataObject.Health;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Impact = vModel.DataObject.Impact;
            obj.PositionResult = vModel.DataObject.PositionResult;
            obj.Recovery = vModel.DataObject.Recovery;
            obj.RekkaKey = vModel.DataObject.RekkaKey;
            obj.RekkaPosition = vModel.DataObject.RekkaPosition;
            obj.Stagger = vModel.DataObject.Stagger;
            obj.Setup = vModel.DataObject.Setup;
            obj.Stamina = vModel.DataObject.Stamina;
            obj.VictimCriteria = vModel.DataObject.VictimCriteria;
            obj.ResultQuality = vModel.DataObject.ResultQuality;
            obj.AdditiveQuality = vModel.DataObject.AdditiveQuality;
            obj.QualityValue = vModel.DataObject.QualityValue;
            obj.Readiness = vModel.DataObject.Readiness;
            obj.ActionVerb = vModel.DataObject.ActionVerb;
            obj.ActionObject = vModel.DataObject.ActionObject;


            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditFightingArt[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
            }

            return RedirectToAction("Index");
        }

        #endregion

        #region Combos
        public ActionResult Combos(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            ManageCombosViewModel vModel = new ManageCombosViewModel(user.GameAccount.Config.Combos)
            {
                AuthedUser = user,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("Combos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCombo(string ID, string authorize)
        {
            string message;
            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
            {
                message = "You must check the proper authorize radio button first.";
            }
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IFightingArtCombination obj = authedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(ID));

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else
                {
                    authedUser.GameAccount.Config.Combos = authedUser.GameAccount.Config.Combos.Where(combo => !combo.Name.Equals(ID));

                    if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUIModule[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Delete Successful.";
                    }
                    else
                    {
                        message = "Error; Removal failed.";
                    }
                }
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddCombos()
        {
            AddEditCombosViewModel vModel = new AddEditCombosViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new FightingArtCombination()
            };

            return View("AddCombos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCombos(AddEditCombosViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            var combos = authedUser.GameAccount.Config.Combos.ToList();

            if(combos.Any(combo => combo.Name.Equals(vModel.DataObject.Name)))
            {
                return RedirectToAction("Combos", new { Message = "Name already taken choose another." });
            }

            combos.Add(vModel.DataObject);

            authedUser.GameAccount.Config.Combos = combos;

            string message;
            if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddCombos[" + vModel.DataObject.Name.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Combos", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditCombos(string id)
        {
            AddEditCombosViewModel vModel = new AddEditCombosViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            IFightingArtCombination obj = vModel.AuthedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(id));

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Combos", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("EditCombos", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditCombos(string id, AddEditCombosViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IFightingArtCombination obj = authedUser.GameAccount.Config.Combos.FirstOrDefault(combo => combo.Name.Equals(id));
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Combos", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Arts = vModel.DataObject.Arts;
            obj.FightingStances = vModel.DataObject.FightingStances;
            obj.IsSystem = vModel.DataObject.IsSystem;
            obj.SituationalUsage = vModel.DataObject.SituationalUsage;

            if (authedUser.GameAccount.Config.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditCombos[" + obj.Name.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Combos", new { Message = message });
        }

        #endregion

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
                AuthedUser = authedUser
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult AddPlaylist()
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IPlayerTemplate currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(chr => chr.Id == authedUser.GameAccount.CurrentlySelectedCharacter);

            AddEditPlaylistViewModel vModel = new AddEditPlaylistViewModel
            {
                AuthedUser = authedUser,
                ValidSongs = ContentUtility.GetMusicTracks()
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

            IPlayerTemplate currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(chr => chr.Id == authedUser.GameAccount.CurrentlySelectedCharacter);
            AddEditPlaylistViewModel vModel = new AddEditPlaylistViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                Name = obj.Name,
                DataObject = obj,
                ValidSongs = ContentUtility.GetMusicTracks()
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