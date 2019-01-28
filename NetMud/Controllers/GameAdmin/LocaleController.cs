using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Locale;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Zone;
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
        [Route(@"Locale/Remove/{zoneId}/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long zoneId, long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                ILocaleTemplate obj = TemplateCache.Get<ILocaleTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveLocale[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                ILocaleTemplate obj = TemplateCache.Get<ILocaleTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveLocale[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

            return RedirectToAction("Edit", "Zone", new { Id = zoneId, Message = message });
        }

        [HttpGet]
        public ActionResult Add(long zoneId)
        {
            IZoneTemplate zone = TemplateCache.Get<IZoneTemplate>(zoneId);

            if(zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            AddEditLocaleTemplateViewModel vModel = new AddEditLocaleTemplateViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ZoneId = zoneId,
                DataObject = new LocaleTemplate()
            };

            return View("~/Views/GameAdmin/Locale/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(long zoneId, AddEditLocaleTemplateViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZoneTemplate zone = TemplateCache.Get<IZoneTemplate>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            LocaleTemplate newObj = new LocaleTemplate
            {
                Name = vModel.DataObject.Name,
                AlwaysDiscovered = vModel.DataObject.AlwaysDiscovered,
                ParentLocation = zone
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
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
            IZoneTemplate zone = TemplateCache.Get<IZoneTemplate>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            string message = string.Empty;
            AddEditLocaleTemplateViewModel vModel = new AddEditLocaleTemplateViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            ILocaleTemplate obj = TemplateCache.Get<ILocaleTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/Locale/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long zoneId, AddEditLocaleTemplateViewModel vModel, long id)
        {
            IZoneTemplate zone = TemplateCache.Get<IZoneTemplate>(zoneId);

            if (zone == null)
            {
                return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = "Invalid zone" });
            }

            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            ILocaleTemplate obj = TemplateCache.Get<ILocaleTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.AlwaysDiscovered = vModel.DataObject.AlwaysDiscovered;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditLocale[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", "Zone", new { Id = zoneId, Message = message });
        }
    }
}