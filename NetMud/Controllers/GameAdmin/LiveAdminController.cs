using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Room;
using NetMud.Models.Admin;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class LiveAdminController : Controller
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

        public LiveAdminController()
        {
        }

        public LiveAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        #region Room
        [HttpGet]
        public ActionResult Rooms(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            LiveRoomsViewModel vModel = new LiveRoomsViewModel(LiveCache.GetAll<IRoom>())
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }

        [HttpGet]
        [Route(@"LiveAdmin/Room")]
        public ActionResult Room(string birthMark, ViewRoomViewModel viewModel)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            ViewRoomViewModel vModel = new ViewRoomViewModel(birthMark)
            {
                AuthedUser = authedUser
            };

            return View(vModel);
        }
        #endregion
    }
}