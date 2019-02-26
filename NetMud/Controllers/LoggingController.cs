using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.Models.Logging;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    [Authorize(Roles = "Admin")]
    public class LoggingController : Controller
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

        public LoggingController()
        {
        }

        public LoggingController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string selectedLog)
        {
            DashboardViewModel dashboardModel = new DashboardViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                ChannelNames = LoggingUtility.GetCurrentLogNames()
            };

            if (!string.IsNullOrWhiteSpace(selectedLog))
            {
                dashboardModel.SelectedLogContent = LoggingUtility.GetCurrentLogContent(selectedLog);
                dashboardModel.SelectedLog = selectedLog;
            }

            return View(dashboardModel);
        }

        [HttpPost]
        public ActionResult Rollover(string selectedLog)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            string message = string.Empty;
            if (!string.IsNullOrWhiteSpace(selectedLog))
            {
                if (!LoggingUtility.RolloverLog(selectedLog))
                {
                    message = "Error rolling over log.";
                }
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RolloverLog[" + selectedLog + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Rollover Successful.";
                }
            }
            else
            {
                message = "No log selected to rollover";
            }

            return RedirectToAction("Index", new { Message = message });

        }
    }
}