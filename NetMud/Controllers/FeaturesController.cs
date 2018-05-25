using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class FeaturesController : Controller
    {
        public ActionResult Races()
        {
            var dataList = BackingDataCache.GetAll<IRace>();

            return View(dataList);
        }

        public ActionResult Flora()
        {
            var dataList = BackingDataCache.GetAll<IFlora>();

            return View(dataList);
        }

        public ActionResult Minerals()
        {
            var dataList = BackingDataCache.GetAll<IMineral>();

            return View(dataList);
        }

        public ActionResult Help()
        {
            var dataList = BackingDataCache.GetAll<IHelp>();

            return View(dataList);
        }

        public ActionResult Skills()
        {
            return View();
        }
    }
}