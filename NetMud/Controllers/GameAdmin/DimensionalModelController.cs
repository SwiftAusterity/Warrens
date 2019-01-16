using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Architectural.EntityBase;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.Models.Admin;
using System;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class DimensionalModelController : Controller
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

        public DimensionalModelController()
        {
        }

        public DimensionalModelController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageDimensionalModelDataViewModel(TemplateCache.GetAll<DimensionalModelData>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/DimensionalModel/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/DimensionalModel/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IDimensionalModelData>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDimensionalModelData[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IDimensionalModelData>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveDimensionalModelData[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add()
        {
            var vModel = new AddEditDimensionalModelDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new DimensionalModelData()
            };

            return View("~/Views/GameAdmin/DimensionalModel/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditDimensionalModelDataViewModel vModel, HttpPostedFileBase modelFile)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                IDimensionalModelData newModel = vModel.DataObject;

                foreach (var plane in newModel.ModelPlanes)
                {
                    foreach (var node in plane.ModelNodes)
                    {
                        node.YAxis = plane.YAxis;
                    }
                }

                if (newModel.IsModelValid())
                {
                    if (newModel.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                    {
                        message = "Error; Creation failed.";
                    }
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddDimensionalModelData[" + newModel.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Creation Successful.";
                    }
                }
                else
                {
                    message = "Invalid model file; Model files must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }


        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditDimensionalModelDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            var obj = TemplateCache.Get<IDimensionalModelData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/DimensionalModel/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditDimensionalModelDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IDimensionalModelData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                foreach (var plane in vModel.DataObject.ModelPlanes)
                {
                    foreach (var node in plane.ModelNodes)
                    {
                        node.YAxis = plane.YAxis;
                    }
                }

                if (vModel.DataObject.IsModelValid())
                {
                    obj.Name = vModel.DataObject.Name;
                    obj.ModelType = vModel.DataObject.ModelType;
                    obj.ModelPlanes = vModel.DataObject.ModelPlanes;

                    if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditDimensionalModelData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                    {
                        message = "Error; Edit failed.";
                    }
                }
                else
                {
                    message = "Invalid model; Models must contain 21 planes of a tag name followed by 21 rows of 21 nodes.";
                }
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex, false);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}