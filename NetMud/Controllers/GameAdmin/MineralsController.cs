using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class MineralsController : Controller
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

        public MineralsController()
        {
        }

        public MineralsController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageMineralsViewModel(BackingDataCache.GetAll<IMineral>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Minerals/Index.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IMineral>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMinerals[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditMineralsViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();
            vModel.ValidMinerals = BackingDataCache.GetAll<IMineral>();

            return View("~/Views/GameAdmin/Minerals/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditMineralsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Mineral();
            newObj.Name = vModel.Name;
            newObj.HelpText = vModel.HelpText;
            newObj.Solubility = vModel.Solubility;
            newObj.Fertility = vModel.Fertility;
            newObj.AmountMultiplier = vModel.AmountMultiplier;
            newObj.Rarity = vModel.Rarity;
            newObj.PuissanceVariance = vModel.PuissanceVariance;
            newObj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            newObj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            newObj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newRock = BackingDataCache.Get<IMaterial>(vModel.Rock);
            if (newRock != null)
                newObj.Rock = newRock;
            else
                message += "Invalid rock material.";

            var newDirt = BackingDataCache.Get<IMaterial>(vModel.Dirt);
            if (newDirt != null)
                newObj.Dirt = newDirt;
            else
                message += "Invalid dirt material.";

            newObj.OccursIn = vModel.OccursIn;
            
            var newOres = new List<IMineral>();
            if (vModel.Ores != null)
            {
                foreach (var mineralId in vModel.Ores)
                {
                    if (mineralId >= 0)
                    {
                        var mineral = BackingDataCache.Get<IMineral>(mineralId);

                        if (mineral != null)
                            newOres.Add(mineral);
                    }
                }

                if (newOres.Count > 0)
                    newObj.Ores = newOres;
            }

            if (!String.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddMinerals[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditMineralsViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidMinerals = BackingDataCache.GetAll<IMineral>().Where(m => m.ID != id);
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();

            var obj = BackingDataCache.Get<IMineral>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText;
            vModel.Solubility = obj.Solubility;
            vModel.Fertility = obj.Fertility;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.Item2;
            vModel.ElevationRangeLow = obj.ElevationRange.Item1;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.Item2;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Item1;
            vModel.HumidityRangeHigh = obj.HumidityRange.Item2;
            vModel.HumidityRangeLow = obj.HumidityRange.Item1;
            vModel.Rock = obj.Rock.ID;
            vModel.Dirt = obj.Dirt.ID;

            return View("~/Views/GameAdmin/Minerals/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditMineralsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IMineral>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.HelpText = vModel.HelpText;
            obj.Solubility = vModel.Solubility;
            obj.Fertility = vModel.Fertility;
            obj.AmountMultiplier = vModel.AmountMultiplier;
            obj.Rarity = vModel.Rarity;
            obj.PuissanceVariance = vModel.PuissanceVariance;
            obj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newDirt = BackingDataCache.Get<IMaterial>(vModel.Dirt);
            if (newDirt != null)
                obj.Dirt = newDirt;
            else
                message += "Invalid dirt material.";

            var newRock = BackingDataCache.Get<IMaterial>(vModel.Rock);
            if (newRock != null)
                obj.Rock = newRock;
            else
                message += "Invalid rock material.";

            var newOres = new List<IMineral>();
            if (vModel.Ores != null)
            {
                foreach (var mineralId in vModel.Ores)
                {
                    if (mineralId >= 0)
                    {
                        var mineral = BackingDataCache.Get<IMineral>(mineralId);

                        if (mineral != null)
                            newOres.Add(mineral);
                    }
                }

                if (newOres.Count > 0)
                    obj.Ores = newOres;
            }

            obj.OccursIn = vModel.OccursIn;

            if (!String.IsNullOrWhiteSpace(message))
            {
                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditMinerals[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}