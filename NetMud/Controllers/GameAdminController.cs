using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Models;
using NetMud.Data.EntityBackingData;
using NetMud.Authentication;
using System.Collections.Generic;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Web.Security;
using NetMud.DataStructure.SupportingClasses;
using System;
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.Models.GameAdmin;

namespace NetMud.Controllers
{
    public class GameAdminController : Controller
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

        public GameAdminController()
        {
        }

        public GameAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }
        //Also called Dashboard in most of the html
        public ActionResult Index()
        {
            var dashboardModel = new DashboardViewModel();
            dashboardModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            var dataWrapper = new DataWrapper();

            dashboardModel.LivePlayers = LiveCache.GetAll<Player>().Count();

            dashboardModel.Inanimates = dataWrapper.GetAll<InanimateData>();
            dashboardModel.Rooms = dataWrapper.GetAll<RoomData>();
            dashboardModel.NPCs = dataWrapper.GetAll<NonPlayerCharacter>();

            return View(dashboardModel);
        }
    }
}
