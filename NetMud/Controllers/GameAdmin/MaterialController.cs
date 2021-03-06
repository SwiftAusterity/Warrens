﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class MaterialController : Controller
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

        public MaterialController()
        {
        }

        public MaterialController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageMaterialDataViewModel vModel = new ManageMaterialDataViewModel(TemplateCache.GetAll<Material>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Material/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"Material/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IMaterial obj = TemplateCache.Get<IMaterial>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMaterial[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IMaterial obj = TemplateCache.Get<IMaterial>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveMaterial[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add(long Template = -1)
        {
            AddEditMaterialViewModel vModel = new AddEditMaterialViewModel(Template)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Material/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditMaterialViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IMaterial newObj = vModel.DataObject;
            string message;
            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddMaterialData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id, string ArchivePath = "")
        {
            Material obj = TemplateCache.Get<Material>(id);

            if (obj == null)
            {
                return RedirectToAction("Index", new { Message = "That does not exist" });
            }

            AddEditMaterialViewModel vModel = new AddEditMaterialViewModel(ArchivePath, obj)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Material/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditMaterialViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            Material obj = TemplateCache.Get<Material>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.Conductive = vModel.DataObject.Conductive;
            obj.Density = vModel.DataObject.Density;
            obj.Ductility = vModel.DataObject.Ductility;
            obj.Flammable = vModel.DataObject.Flammable;
            obj.GasPoint = vModel.DataObject.GasPoint;
            obj.Magnetic = vModel.DataObject.Magnetic;
            obj.Mallebility = vModel.DataObject.Mallebility;
            obj.Porosity = vModel.DataObject.Porosity;
            obj.SolidPoint = vModel.DataObject.SolidPoint;
            obj.TemperatureRetention = vModel.DataObject.TemperatureRetention;
            obj.Viscosity = vModel.DataObject.Viscosity;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Resistance = vModel.DataObject.Resistance;
            obj.Composition = vModel.DataObject.Composition;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditMaterialData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}