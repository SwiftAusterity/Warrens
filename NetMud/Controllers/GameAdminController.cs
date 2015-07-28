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

            dashboardModel.LivePlayers = LiveCache.GetAll<Player>().Count();

            dashboardModel.Inanimates = DataWrapper.GetAll<InanimateData>();
            dashboardModel.Rooms = DataWrapper.GetAll<RoomData>();
            dashboardModel.NPCs = DataWrapper.GetAll<NonPlayerCharacter>();

            return View(dashboardModel);
        }

        public ActionResult ManageInanimateData()
        {
            var vModel = new ManageInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.Inanimates = DataWrapper.GetAll<InanimateData>();

            return View(vModel);
        }

        public ActionResult ManageRoomData()
        {
            var vModel = new ManageRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.Rooms = DataWrapper.GetAll<RoomData>();

            return View(vModel);
        }

        public ActionResult ManageNPCData()
        {
            var vModel = new ManageNPCDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.NPCs = DataWrapper.GetAll<NonPlayerCharacter>();

            return View(vModel);
        }

        //TODO: This, we really need to be looking at "users" not players
        public ActionResult ManagePlayers()
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            var vModel = new ManagePlayersViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.Players = UserManager.Users.ToList();
            vModel.ValidRoles = roleManager.Roles.ToList();

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveInanimate(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var userId = User.Identity.GetUserId();
                var model = new ManageRoomDataViewModel
                {
                    authedUser = UserManager.FindById(userId)
                };

                var obj = DataWrapper.GetOne<InanimateData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInanimate[" + ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveNPC(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var userId = User.Identity.GetUserId();
                var model = new ManageRoomDataViewModel
                {
                    authedUser = UserManager.FindById(userId)
                };

                var obj = DataWrapper.GetOne<NonPlayerCharacter>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveNPC[" + ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageNPCData", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRoom(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var userId = User.Identity.GetUserId();
                var model = new ManageRoomDataViewModel
                {
                    authedUser = UserManager.FindById(userId)
                };

                var obj = DataWrapper.GetOne<RoomData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageRoomData", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePlayer(long ID, string authorize)
        {
            string message = "Not Implimented";

            return RedirectToAction("ManagePlayers", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInanimateData(string newName)
        {
            string message = string.Empty;
            var userId = User.Identity.GetUserId();
            var model = new ManageInanimateDataViewModel
            {
                authedUser = UserManager.FindById(userId)
            };

            var newObj = new InanimateData();
            newObj.Name = newName;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateData[" + newObj.ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNPCData(string newName, string newSurName, string newGender, StaffRank chosenRole = StaffRank.Player)
        {
            string message = string.Empty;
            var userId = User.Identity.GetUserId();
            var model = new ManageNPCDataViewModel
            {
                authedUser = UserManager.FindById(userId)
            };

            var newObj = new Character();
            newObj.Name = newName;
            newObj.SurName = newSurName;
            newObj.Gender = newGender;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddNPCData[" + newObj.ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageNPCData", new { Message = message });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoomData(string newName)
        {
            string message = string.Empty;
            var userId = User.Identity.GetUserId();
            var model = new ManageRoomDataViewModel
            {
                authedUser = UserManager.FindById(userId)
            };

            var newObj = new RoomData();
            newObj.Name = newName;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomData[" + newObj.ID.ToString() + "]", model.authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageRoomData", new { Message = message });
        }
    }
}
