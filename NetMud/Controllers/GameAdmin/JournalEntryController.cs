using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class JournalEntryController : Controller
    {
        // GET: JournalEntry
        public ActionResult Index()
        {
            return View();
        }
    }
}