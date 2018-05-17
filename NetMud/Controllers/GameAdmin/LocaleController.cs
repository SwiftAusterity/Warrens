using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Place;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
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
        public ActionResult Remove(long zoneId, long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<ILocaleData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveLocale[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

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
                Affiliation = zone
            };

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                zone.Locales.Add(newObj);
                zone.Save();

                LoggingUtility.LogAdminCommandUsage("*WEB* - AddLocale[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLocale[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
        }
    }
}