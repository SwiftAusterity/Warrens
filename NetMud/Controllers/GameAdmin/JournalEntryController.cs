using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class JournalEntryController : Controller
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

        public JournalEntryController()
        {
        }

        public JournalEntryController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageJournalEntriesViewModel(BackingDataCache.GetAll<IJournalEntry>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/JournalEntry/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/JournalEntry/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IJournalEntry>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveJournalEntry[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IJournalEntry>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveJournalEntry[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                    message = "Error; Unapproval failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditJournalEntryViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                PublishDate = DateTime.Now.AddDays(1).ToShortDateString(),
                ExpireDate = DateTime.Now.AddDays(30).ToShortDateString(),
                Public = true,
                Expired = false,
                Body = string.Empty,
                MinimumReadLevel = (short)StaffRank.Player,
                Name = "",
                Tags = ""
            };

            return View("~/Views/GameAdmin/JournalEntry/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditJournalEntryViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var tagList = new List<string>();

            foreach (var tag in vModel.Tags.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                tagList.Add(tag);

            var newObj = new JournalEntry
            {
                Name = vModel.Name,
                Body = vModel.Body,
                Expired = vModel.Expired,
                ExpireDate = DateTime.Parse(vModel.ExpireDate),
                MinimumReadLevel = (StaffRank)vModel.MinimumReadLevel,
                Public = vModel.Public,
                PublishDate = DateTime.Parse(vModel.PublishDate),
                Tags = tagList.ToArray()
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddJournalEntry[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditJournalEntryViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = BackingDataCache.Get<IJournalEntry>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.Body = obj.Body.Value;
            vModel.Expired = obj.Expired;
            vModel.ExpireDate = obj.ExpireDate.ToShortDateString();
            vModel.MinimumReadLevel = (short)obj.MinimumReadLevel;
            vModel.Public = obj.Public;
            vModel.PublishDate = obj.PublishDate.ToShortDateString();

            return View("~/Views/GameAdmin/JournalEntry/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditJournalEntryViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IJournalEntry>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                obj.Name = vModel.Name;
                obj.Body = vModel.Body;
                obj.Expired = vModel.Expired;
                obj.ExpireDate = DateTime.Parse(vModel.ExpireDate);
                obj.MinimumReadLevel = (StaffRank)vModel.MinimumReadLevel;
                obj.Public = vModel.Public;
                obj.PublishDate = DateTime.Parse(vModel.PublishDate);

                var tagList = new List<string>();

                foreach (var tag in vModel.Tags.Split(new string[] { "||" }, StringSplitOptions.RemoveEmptyEntries))
                    tagList.Add(tag);

                obj.Tags = tagList.ToArray();

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditJournalEntry[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }
            catch
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}