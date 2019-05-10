using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Game;
using NetMud.Models.Admin;
using System.IO;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class GameTemplateController : Controller
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

        public GameTemplateController()
        {
        }

        public GameTemplateController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            ManageGameViewModel vModel = new ManageGameViewModel(TemplateCache.GetAll<IGameTemplate>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/GameTemplate/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameTemplate/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                IGameTemplate obj = TemplateCache.Get<IGameTemplate>(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveGameTemplate[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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

                IGameTemplate obj = TemplateCache.Get<IGameTemplate>(unapproveId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Unapproved))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveGameTemplate[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            AddEditGameTemplateViewModel vModel = new AddEditGameTemplateViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = new GameTemplate()
            };

            return View("~/Views/GameAdmin/GameTemplate/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditGameTemplateViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = vModel.DataObject;

            string luaCode = string.Empty;
            using (StreamReader reader = new StreamReader(vModel.LuaEngine.FileContent))
            {
                luaCode = reader.ReadToEnd();
            }

            string message;
            if (!LuaUtility.Validate(luaCode))
            {
                message = "Invalid LUA game code file. See Game Submission page for more details on how to construct game logic.";
            }
            else
            {
                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    //TODO: Save the lua code to a file

                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddGameTemplate[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            AddEditGameTemplateViewModel vModel = new AddEditGameTemplateViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            IGameTemplate obj = TemplateCache.Get<IGameTemplate>(id);

            if (obj == null)
            {
                string message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;

            return View("~/Views/GameAdmin/GameTemplate/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditGameTemplateViewModel vModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IGameTemplate obj = TemplateCache.Get<IGameTemplate>(id);
            string message;
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            try
            {
                string luaCode = string.Empty;
                using (StreamReader reader = new StreamReader(vModel.LuaEngine.FileContent))
                {
                    luaCode = reader.ReadToEnd();
                }

                obj.Name = vModel.DataObject.Name;
                obj.AverageDuration = vModel.DataObject.AverageDuration;
                obj.Description = vModel.DataObject.Description;
                obj.HighScoreboard = vModel.DataObject.HighScoreboard;
                obj.NumberOfPlayers =vModel.DataObject.NumberOfPlayers;
                obj.PublicReplay = vModel.DataObject.PublicReplay;
                obj.TurnDuration = vModel.DataObject.TurnDuration;

                if (!LuaUtility.Validate(luaCode))
                {
                    message = "Invalid LUA game code file. See Game Submission page for more details on how to construct game logic.";
                }
                else
                {
                    if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                    {
                        //TODO: Save the lua code to a file

                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditGameTemplate[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else
                    {
                        message = "Error; Edit failed.";
                    }
                }
            }
            catch
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}