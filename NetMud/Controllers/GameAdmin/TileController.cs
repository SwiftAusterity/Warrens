using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Action;
using NetMud.Data.Tiles;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Action;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Tile;
using NetMud.Models;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using ActionResult = System.Web.Mvc.ActionResult;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class TileController : Controller
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

        public TileController()
        {
        }

        public TileController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageTileDataViewModel vModel = new ManageTileDataViewModel(TemplateCache.GetAll<ITileTemplate>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Tile/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Tile/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<ITileTemplate>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveTile[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<ITileTemplate>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveTile[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditTileDataViewModel vModel = new AddEditTileDataViewModel(Template)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new TileTemplate()
            };

            return View("~/Views/GameAdmin/Tile/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditTileDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            TileTemplate obj = new TileTemplate
            {
                Name = vModel.DataObject.Name,
                HexColorCode = vModel.DataObject.HexColorCode,
                AsciiCharacter = vModel.DataObject.AsciiCharacter,
                Description = vModel.DataObject.Description,
                Air = vModel.DataObject.Air,
                Pathable = vModel.DataObject.Pathable,
                Aquatic = vModel.DataObject.Aquatic,
                BackgroundHexColor = vModel.DataObject.BackgroundHexColor
            };

            if (obj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) != null)
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddTileData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id, long Template = -1)
        {
            string message = string.Empty;
            AddEditTileDataViewModel vModel = new AddEditTileDataViewModel(-1)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            TileTemplate obj = TemplateCache.Get<TileTemplate>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToRoute("ErrorOrClose", new { Message = message });
            }

            vModel.DataObject = obj;

            //Apply the Actions from this other guy, dont suck in actions from ourself thats stupid
            if (Template > -1 && id != Template)
            {
                bool changedActions = false;
                var DataTemplate = TemplateCache.Get<ITileTemplate>(Template);

                List<IDecayEvent> decayEvents = new List<IDecayEvent>();
                foreach (var action in DataTemplate.DecayEvents)
                {
                    decayEvents.Add((IDecayEvent)action.Clone());
                    changedActions = true;
                }
                obj.DecayEvents = new HashSet<IDecayEvent>(decayEvents);

                List<IInteraction> interactions = new List<IInteraction>();
                foreach (var action in DataTemplate.Interactions)
                {
                    interactions.Add((IInteraction)action.Clone());
                    changedActions = true;
                }
                obj.Interactions = new HashSet<IInteraction>(interactions);

                if (changedActions)
                {
                    if (obj.Save(vModel.authedUser.GameAccount, vModel.authedUser.GetStaffRank(User)))
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.Id.ToString() + "]", vModel.authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                        message = "Error; Edit failed.";
                }
            }

            return View("~/Views/GameAdmin/Tile/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditTileDataViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            TileTemplate obj = TemplateCache.Get<TileTemplate>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.DataObject.Name;
            obj.HexColorCode = vModel.DataObject.HexColorCode;
            obj.AsciiCharacter = vModel.DataObject.AsciiCharacter;
            obj.Description = vModel.DataObject.Description;
            obj.Air = vModel.DataObject.Air;
            obj.Pathable = vModel.DataObject.Pathable;
            obj.Aquatic = vModel.DataObject.Aquatic;
            obj.BackgroundHexColor = vModel.DataObject.BackgroundHexColor;

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditTileData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        #region Actions
        [HttpGet]
        public ActionResult AddInteraction(long id)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            AddEditInteractionViewModel vModel = new AddEditInteractionViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new Interaction(),
                ParentObject = origin,
                ClassType = "Tile"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInteraction(long id, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddInteraction(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public ActionResult EditInteraction(long id, string actionName)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            var action = origin.Interactions.FirstOrDefault(act => act.Name.Equals(actionName, System.StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditInteractionViewModel vModel = new AddEditInteractionViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = action,
                ParentObject = origin,
                ClassType = "Tile"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInteraction(long id, string actionName, AddEditInteractionViewModel vModel)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            var action = origin.Interactions.FirstOrDefault(act => act.Name.Equals(actionName, System.StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditInteraction(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveInteraction(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var origin = TemplateCache.Get<ITileTemplate>(id);

                    message = ActionUtility.RemoveInteraction(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpGet]
        public ActionResult AddDecayEvent(long id)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new DecayEvent(),
                ParentObject = origin,
                ClassType = "Tile"
            };

            return View("AddDecayEvent", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDecayEvent(long id, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            return RedirectToAction("Edit", new { Message = ActionUtility.AddDecayEvent(origin, vModel.DataObject, authedUser, User), id });
        }

        [HttpGet]
        public ActionResult EditDecayEvent(long id, string actionName)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, System.StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            AddEditDecayEventViewModel vModel = new AddEditDecayEventViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = action,
                ParentObject = origin,
                ClassType = "Tile"
            };

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditDecayEvent(long id, string actionName, AddEditDecayEventViewModel vModel)
        {
            var origin = TemplateCache.Get<ITileTemplate>(id);
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (origin == null)
                return RedirectToAction("Index", new { Message = "Invalid Tile" });

            var action = origin.DecayEvents.FirstOrDefault(act => act.Name.Equals(actionName, System.StringComparison.InvariantCultureIgnoreCase));

            if (action == null)
            {
                return RedirectToAction("Edit", new { Message = "That is an invalid action.", id });
            }

            return RedirectToAction("Edit", new { Message = ActionUtility.EditDecayEvent(origin, action, vModel.DataObject, authedUser, User), id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDecayEvent(long id, string removeId, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                if (!authorize.Equals(removeId))
                    message = "You must check the proper authorize radio button first.";
                else
                {
                    var origin = TemplateCache.Get<ITileTemplate>(id);

                    message = ActionUtility.RemoveDecayEvent(origin, authorize, authedUser, User);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }
        #endregion
    }
}