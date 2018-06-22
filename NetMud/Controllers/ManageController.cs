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
            var config = user.GameAccount.Config;

            var model = new ManageAccountViewModel
            {
                authedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle
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

            if (obj.Config.Save())
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
                else if (character.Remove())
                    message = "Character successfully deleted.";
                else
                    message = "Error. Character not removed.";
            }

            return RedirectToAction("ManageCharacters", new { Message = message });
        }

        [HttpPost]
        public ActionResult ToggleTutorialMode(bool state)
        {
            string message = string.Empty;

            var userId = User.Identity.GetUserId();
            var authedUser = UserManager.FindById(userId);
            var account = authedUser.GameAccount;

            if (account == null)
                message = "That account does not exist";
            else
            {
                account.Config.UITutorialMode = state;
                account.Config.Save();
            }

            return new EmptyResult();
        }

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