using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class FeaturesController : Controller
    {
        public ActionResult Races()
        {
            var races = BackingDataCache.GetAll<IRace>();

            return View(races);
        }

        public ActionResult Skills()
        {
            return View();
        }
    }
}