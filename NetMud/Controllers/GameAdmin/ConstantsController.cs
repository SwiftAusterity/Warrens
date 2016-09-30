using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
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
            var vModel = new ManageConstantsDataViewModel(BackingDataCache.GetAll<IConstants>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Constants/Index.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IConstants>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveConstantsFile[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditConstantsViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View("~/Views/GameAdmin/Constants/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditConstantsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new NetMud.Data.System.Constants();
            newObj.Name = vModel.NewName;

            var criterion = new Dictionary<CriteriaType, string>();
            if (vModel.NewCriterionTypes != null && vModel.NewCriterionValues != null)
            {
                int nameIndex = 0;
                foreach (var criteriaType in vModel.NewCriterionTypes)
                {
                    if (criteriaType >= 0)
                    {
                        if (vModel.NewCriterionValues.Length <= nameIndex)
                            break;

                        var criteriaValue = vModel.NewCriterionValues[nameIndex];
                        if (!string.IsNullOrWhiteSpace(criteriaValue))
                            criterion.Add((CriteriaType)criteriaType, criteriaValue);
                    }

                    nameIndex++;
                }
            }

            var newValues = new HashSet<string>();
            if (vModel.NewConstantValues != null)
                foreach (var newValue in vModel.NewConstantValues)
                    if (!string.IsNullOrWhiteSpace(newValue) && !newValues.Contains(newValue))
                        newValues.Add(newValue);

            if(criterion == null || criterion.Count == 0 || newValues == null || newValues.Count == 0)
            {
                message = "You must supply at least one criteria and one value to create a new constant cluster.";
                return RedirectToAction("Index", new { Message = message });
            }

            var newLookup = new LookupCriteria();
            newLookup.Criterion = criterion;

            newObj.AddOrUpdate(newLookup, newValues);

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddConstantsFile[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditConstantsViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IConstants>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;

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

            obj.Name = vModel.NewName;

            var criterion = new Dictionary<CriteriaType, string>();
            if (vModel.NewCriterionTypes != null && vModel.NewCriterionValues != null)
            {
                int nameIndex = 0;
                foreach (var criteriaType in vModel.NewCriterionTypes)
                {
                    if (criteriaType >= 0)
                    {
                        if (vModel.NewCriterionValues.Length <= nameIndex)
                            break;

                        var criteriaValue = vModel.NewCriterionValues[nameIndex];
                        if (!string.IsNullOrWhiteSpace(criteriaValue))
                            criterion.Add((CriteriaType)criteriaType, criteriaValue);
                    }

                    nameIndex++;
                }
            }

            var newValues = new HashSet<string>();
            if (vModel.NewConstantValues != null)
                foreach (var newValue in vModel.NewConstantValues)
                    if (!string.IsNullOrWhiteSpace(newValue) && !newValues.Contains(newValue))
                        newValues.Add(newValue);

            var newLookup = new LookupCriteria();
            newLookup.Criterion = criterion;

            obj.AddOrUpdate(newLookup, newValues);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditConstantsFile[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}