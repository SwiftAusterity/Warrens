using NetMud.DataAccess;
using NetMud.Models;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
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
            var vModel = new BugReportModel();

            return View("~/Views/Shared/ReportBug.cshtml", "_chromelessLayout", vModel);
        }

        [HttpPost]
        public ActionResult ReportBug(string body)
        {
            if (!string.IsNullOrWhiteSpace(body))
                LoggingUtility.Log(body, LogChannels.BugReport, true);

            return RedirectToRoute("ModalErrorOrClose", new { Message = "" });
        }
    }
}