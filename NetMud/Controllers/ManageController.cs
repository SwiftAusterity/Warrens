using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using System;
using NetMud.Data.LookupData;
using NetMud.Models;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.SupportingClasses;
using NetMud.DataAccess;
using NetMud.Models.Admin;
using NetMud.DataStructure.Base.PlayerConfiguration;

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
            var user = UserManager.FindById(User.Identity.GetUserId());
            var account = user.GameAccount;

            var model = new ManageAccountViewModel
            {
                authedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle,
                UIModuleCount = BackingDataCache.GetAll<IUIModule>(true).Count(uimod => uimod.CreatorHandle.Equals(account.GlobalIdentityHandle))
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccountConfig(ManageAccountViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            var obj = authedUser.GameAccount;

            obj.Config.UITutorialMode = vModel.UITutorialMode;

            if(vModel.LogChannelSubscriptions != null)
                obj.LogChannelSubscriptions = vModel.LogChannelSubscriptions;

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


        [HttpGet]
        public ActionResult ManageCharacters(string message)
        {
            ViewBag.StatusMessage = message;

            var userId = User.Identity.GetUserId();
            var model = new ManageCharactersViewModel
            {
                authedUser = UserManager.FindById(userId),
                ValidRoles = (StaffRank[])Enum.GetValues(typeof(StaffRank))
            };

            model.ValidRaces = BackingDataCache.GetAll<Race>();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCharacter(string Name, string SurName, string Gender, long raceId, StaffRank chosenRole = StaffRank.Player)
        {
            string message = string.Empty;
            var userId = User.Identity.GetUserId();
            var model = new ManageCharactersViewModel
            {
                authedUser = UserManager.FindById(userId)
            };

            var newChar = new Character
            {
                Name = Name,
                SurName = SurName,
                Gender = Gender
            };
            var race = BackingDataCache.Get<Race>(raceId);

            if (race != null)
                newChar.RaceData = race;

            if (User.IsInRole("Admin"))
                newChar.GamePermissionsRank = chosenRole;
            else
                newChar.GamePermissionsRank = StaffRank.Player;

            message = model.authedUser.GameAccount.AddCharacter(newChar);

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveCharacter(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {

                var userId = User.Identity.GetUserId();
                var model = new ManageCharactersViewModel
                {
                    authedUser = UserManager.FindById(userId)
                };

                var character = model.authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.Id.Equals(ID));

                if (character == null)
                    message = "That character does not exist";
                else if (character.Remove(model.authedUser.GameAccount, model.authedUser.GetStaffRank(User)))
                    message = "Character successfully deleted.";
                else
                    message = "Error. Character not removed.";
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        #region UIModules
        public ActionResult UIModules(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            var vModel = new ManageUIModulesViewModel(BackingDataCache.GetAll<IUIModule>().Where(uimod => uimod.CreatorHandle.Equals(user.GameAccount.GlobalIdentityHandle)))
            {
                authedUser = user,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("UIModules", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveUIModule(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IUIModule>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveUIModule[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddUIModule()
        {
            var vModel = new AddEditUIModuleViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("AddUIModule", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddUIModule(AddEditUIModuleViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new UIModule
            {
                Name = vModel.Name,
                BodyHtml = vModel.BodyHtml,
                Height = vModel.Height,
                Width = vModel.Width,
                HelpText = vModel.HelpText
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddUIModule[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("UIModules", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditUIModule(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditUIModuleViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = BackingDataCache.Get<IUIModule>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("UIModules", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.BodyHtml = obj.BodyHtml;
            vModel.Height = obj.Height;
            vModel.Width = obj.Width;
            vModel.HelpText = obj.HelpText;

            return View("EditUIModule", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditUIModule(long id, AddEditUIModuleViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IUIModule>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("UIModules", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BodyHtml = vModel.BodyHtml;
            obj.Height = vModel.Height;
            obj.Width = vModel.Width;
            obj.HelpText = vModel.HelpText;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditUIModule[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("UIModules", new { Message = message });
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
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
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
                var result = await UserManager.AddPasswordAsync(User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
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
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }
        #endregion
    }
}