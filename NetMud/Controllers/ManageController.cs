using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.PlayerManagement;
using System;
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

            var wordQuery = new FilteredQuery<ILanguage>(CacheType.ConfigData)
            {
                Filter = lang => lang.SuitableForUse && lang.UIOnly
            };

            ManageAccountViewModel model = new ManageAccountViewModel
            {
                AuthedUser = user,
                DataObject = account,
                GlobalIdentityHandle = account.GlobalIdentityHandle,
                UILanguage = account.Config.UILanguage,
                ChosenRole = user.GetStaffRank(User),
                ValidRoles = (StaffRank[])Enum.GetValues(typeof(StaffRank)),
                ValidLanguages = wordQuery.FilteredItems
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditAccountConfig(ManageAccountViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            Account obj = authedUser.GameAccount;

            obj.Config.UILanguage = vModel.UILanguage;

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