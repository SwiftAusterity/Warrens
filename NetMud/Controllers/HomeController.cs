using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.Models;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class HomeController : Controller
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

        public HomeController()
        {
        }

        public HomeController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index()
        {
            ApplicationUser user = null;
            HomeViewModel vModel = new HomeViewModel();

            try
            {
                IEnumerable<IJournalEntry> validEntries;
                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
                }
                else
                {
                    validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
                }

                vModel.AuthedUser = user;
                vModel.LatestNews = validEntries.Where(blog => !blog.HasTag("Patch Notes")).OrderByDescending(blog => blog.PublishDate).Take(3);
                vModel.LatestPatchNotes = validEntries.OrderByDescending(blog => blog.PublishDate).FirstOrDefault(blog => blog.HasTag("Patch Notes"));
            }
            catch
            {
                vModel.AuthedUser = user;
                vModel.LatestNews = Enumerable.Empty<IJournalEntry>();
                vModel.LatestPatchNotes = null;
            }

            return View(vModel);
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Contact()
        {
            return View();
        }

        [HttpGet]
        public ActionResult ReportBug()
        {
            BugReportModel vModel = new BugReportModel();

            return View("~/Views/Shared/ReportBug.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        public ActionResult ReportBug(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                LoggingUtility.Log(body, LogChannels.BugReport, true);
            }

            return RedirectToRoute("ModalErrorOrClose", new { Message = "" });
        }
    }
}