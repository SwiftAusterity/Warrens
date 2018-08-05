using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class LocaleController : Controller
    {
        private ApplicationUserManager _userManager;
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

        public LocaleController()
        {
        }

        public LocaleController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Locale/Remove/{zoneId}/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long zoneId, long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<ILocaleData>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveLocale[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<ILocaleData>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveLocale[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                    message = "Error; Unapproval failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

            return RedirectToAction("Edit", "Zone", new { Id = zoneId, Message = message });
        }

        [HttpGet]
        public ActionResult Add(long zoneId)
        {
            IZoneData zone = BackingDataCache.Get<IZoneData>(zoneId);

            if(zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            var vModel = new AddEditLocaleDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ZoneId = zoneId
            };

            return View("~/Views/GameAdmin/Locale/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long zoneId, AddEditLocaleDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZoneData zone = BackingDataCache.Get<IZoneData>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            var newObj = new LocaleData
            {
                Name = vModel.Name,
                ParentLocation = zone
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddLocale[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long zoneId, long id)
        {
            IZoneData zone = BackingDataCache.Get<IZoneData>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            string message = string.Empty;
            var vModel = new AddEditLocaleDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            ILocaleData obj = BackingDataCache.Get<ILocaleData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            return View("~/Views/GameAdmin/Locale/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long zoneId, AddEditLocaleDataViewModel vModel, long id)
        {
            IZoneData zone = BackingDataCache.Get<IZoneData>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILocaleData obj = BackingDataCache.Get<ILocaleData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLocale[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
        }
    }
}