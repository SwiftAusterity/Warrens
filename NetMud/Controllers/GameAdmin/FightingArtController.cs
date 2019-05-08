using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Combat;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Combat;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class FightingArtController : Controller
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

        public FightingArtController()
        {
        }

        public FightingArtController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageFightingArtViewModel vModel = new ManageFightingArtViewModel(TemplateCache.GetAll<IFightingArt>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/FightingArt/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"FightingArt/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IFightingArt obj = TemplateCache.Get<IFightingArt>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IFightingArt obj = TemplateCache.Get<IFightingArt>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveRoom[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditFightingArtViewModel vModel = new AddEditFightingArtViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new FightingArt()
            };

            return View("~/Views/GameAdmin/FightingArt/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditFightingArtViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IFightingArt newObj = vModel.DataObject;

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
            {
                message = "Error; Creation failed.";
            }
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddFightingArt[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            AddEditFightingArtViewModel vModel = new AddEditFightingArtViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = obj,
            };

            return View("~/Views/GameAdmin/FightingArt/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditFightingArtViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IFightingArt obj = TemplateCache.Get<IFightingArt>(id);
            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToRoute("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.ActorCriteria = vModel.DataObject.ActorCriteria;
            obj.Aim = vModel.DataObject.Aim;
            obj.Armor = vModel.DataObject.Armor;
            obj.DistanceChange = vModel.DataObject.DistanceChange;
            obj.DistanceRange = vModel.DataObject.DistanceRange;
            obj.Health = vModel.DataObject.Health;
            obj.HelpText = vModel.DataObject.HelpText;
            obj.Impact = vModel.DataObject.Impact;
            obj.PositionResult = vModel.DataObject.PositionResult;
            obj.Recovery = vModel.DataObject.Recovery;
            obj.RekkaKey = vModel.DataObject.RekkaKey;
            obj.RekkaPosition = vModel.DataObject.RekkaPosition;
            obj.Setup = vModel.DataObject.Setup;
            obj.Stamina = vModel.DataObject.Stamina;
            obj.VictimCriteria = vModel.DataObject.VictimCriteria;
            obj.ResultQuality = vModel.DataObject.ResultQuality;
            obj.AdditiveQuality = vModel.DataObject.AdditiveQuality;
            obj.QualityValue = vModel.DataObject.QualityValue;
            obj.Readiness = vModel.DataObject.Readiness;
            obj.ActionVerb = vModel.DataObject.ActionVerb;
            obj.ActionObject = vModel.DataObject.ActionObject;


            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditFightingArt[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
            {
            }

            return RedirectToAction("Index");
        }
    }
}