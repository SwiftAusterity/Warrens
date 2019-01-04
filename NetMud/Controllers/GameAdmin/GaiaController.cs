using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Gaias;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Gaia;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class GaiaController : Controller
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

        public GaiaController()
        {
        }

        public GaiaController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageGaiaViewModel vModel = new ManageGaiaViewModel(TemplateCache.GetAll<IGaiaTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Gaia/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Gaia/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IGaiaTemplate>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveGaiaData[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IGaiaTemplate>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveGaiaData[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
        public ActionResult Add(long Template = -1)
        {
            AddEditGaiaViewModel vModel = new AddEditGaiaViewModel(Template)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidCelestials = TemplateCache.GetAll<ICelestial>(true),
                DataObject = new GaiaTemplate()
            };

            return View("~/Views/GameAdmin/Gaia/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditGaiaViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            GaiaTemplate newObj = new GaiaTemplate
            {
                Name = vModel.DataObject.Name,
                RotationalAngle = vModel.DataObject.RotationalAngle,
                CelestialBodies = vModel.DataObject.CelestialBodies,
                ChronologicalSystem = vModel.DataObject.ChronologicalSystem
            };

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddGaiaData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            AddEditGaiaViewModel vModel = new AddEditGaiaViewModel(-1)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidCelestials = TemplateCache.GetAll<ICelestial>(true)
            };

            var obj = TemplateCache.Get<GaiaTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/Gaia/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditGaiaViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IGaiaTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                obj.Name = vModel.DataObject.Name;
                obj.CelestialBodies = vModel.DataObject.CelestialBodies;
                obj.ChronologicalSystem = vModel.DataObject.ChronologicalSystem;
                obj.RotationalAngle = vModel.DataObject.RotationalAngle;

                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditGaiaData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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