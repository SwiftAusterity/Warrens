using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.DataAccess;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize]
    public class PlayerController : Controller
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

        public PlayerController()
        {
        }

        public PlayerController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }


        [HttpPost]
        [Route(@"Player/SelectCharacter/{id}")]
        public JsonResult SelectCharacter(long id)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (authedUser != null && id >= 0)
            {
                authedUser.GameAccount.CurrentlySelectedCharacter = id;
                UserManager.Update(authedUser);
            }

            return new JsonResult();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            ManagePlayersViewModel vModel = new ManagePlayersViewModel(UserManager.Users)
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms,

                ValidRoles = roleManager.Roles.ToList()
            };

            return View("~/Views/GameAdmin/Player/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles="Admin")]
        [Route(@"Player/Remove/{removeId?}/{authorizeRemove?}")]
        public ActionResult Remove(string removeId, string authorizeRemove)
        {
            string message;
            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

                DataStructure.Player.IAccount obj = Account.GetByHandle(removeId);

                if (obj == null)
                {
                    message = "That does not exist";
                }
                else if (obj.Delete(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveAccount[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                {
                    message = "Error; Removal failed.";
                }
            }
            else
            {
                message = "You must check the proper remove or unapprove authorization radio button first.";
            }

            return RedirectToAction("Index", new { Message = message });

        }
    }
}