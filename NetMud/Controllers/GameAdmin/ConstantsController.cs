using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class ConstantsController : Controller
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

        public ConstantsController()
        {
        }

        public ConstantsController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageConstantsDataViewModel(BackingDataCache.GetAll<IConstants>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Constants/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Constants/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IConstants>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstants[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IConstants>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveConstants[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditConstantsViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Constants/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditConstantsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Data.System.Constants
            {
                Name = vModel.Name
            };

            var criterion = new Dictionary<CriteriaType, string>();
            if (vModel.CriterionTypes != null && vModel.CriterionValues != null)
            {
                int nameIndex = 0;
                foreach (var criteriaType in vModel.CriterionTypes)
                {
                    if (criteriaType >= 0)
                    {
                        if (vModel.CriterionValues.Length <= nameIndex)
                            break;

                        var criteriaValue = vModel.CriterionValues[nameIndex];
                        if (!string.IsNullOrWhiteSpace(criteriaValue))
                            criterion.Add((CriteriaType)criteriaType, criteriaValue);
                    }

                    nameIndex++;
                }
            }

            var newValues = new HashSet<string>();
            if (vModel.ConstantValues != null)
                foreach (var newValue in vModel.ConstantValues)
                    if (!string.IsNullOrWhiteSpace(newValue) && !newValues.Contains(newValue))
                        newValues.Add(newValue);

            if(criterion == null || criterion.Count == 0 || newValues == null || newValues.Count == 0)
            {
                message = "You must supply at least one criteria and one value to create a new constant cluster.";
                return RedirectToAction("Index", new { Message = message });
            }

            var newLookup = new LookupCriteria
            {
                Criterion = criterion
            };

            newObj.AddOrUpdate(newLookup, newValues);

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddConstantsFile[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditConstantsViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = BackingDataCache.Get<IConstants>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;

            return View("~/Views/GameAdmin/Constants/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditConstantsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IConstants>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;

            var criterion = new Dictionary<CriteriaType, string>();
            if (vModel.CriterionTypes != null && vModel.CriterionValues != null)
            {
                int nameIndex = 0;
                foreach (var criteriaType in vModel.CriterionTypes)
                {
                    if (criteriaType >= 0)
                    {
                        if (vModel.CriterionValues.Length <= nameIndex)
                            break;

                        var criteriaValue = vModel.CriterionValues[nameIndex];
                        if (!string.IsNullOrWhiteSpace(criteriaValue))
                            criterion.Add((CriteriaType)criteriaType, criteriaValue);
                    }

                    nameIndex++;
                }
            }

            var newValues = new HashSet<string>();
            if (vModel.ConstantValues != null)
                foreach (var newValue in vModel.ConstantValues)
                    if (!string.IsNullOrWhiteSpace(newValue) && !newValues.Contains(newValue))
                        newValues.Add(newValue);

            var newLookup = new LookupCriteria
            {
                Criterion = criterion
            };

            obj.AddOrUpdate(newLookup, newValues);

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditConstantsFile[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}