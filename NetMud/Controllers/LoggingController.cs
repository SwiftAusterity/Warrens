using NetMud.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Models.Logging;
using NetMud.DataAccess;

namespace NetMud.Controllers
{
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
            var dashboardModel = new DashboardViewModel();
            dashboardModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            dashboardModel.ChannelNames = LoggingUtility.GetCurrentLogNames();

            if (!String.IsNullOrWhiteSpace(selectedLog))
            {
                dashboardModel.SelectedLogContent = LoggingUtility.GetCurrentLogContent(selectedLog);
                dashboardModel.SelectedLog = selectedLog;
            }

            return View(dashboardModel);
        }
    }
}