using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class FaunaController : Controller
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

        public FaunaController()
        {
        }

        public FaunaController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageFaunaViewModel(BackingDataCache.GetAll<IFauna>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Fauna/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<IFauna>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveFauna[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditFaunaViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();
            vModel.ValidRaces = BackingDataCache.GetAll<IRace>();

            return View("~/Views/GameAdmin/Fauna/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditFaunaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Fauna();
            newObj.Name = vModel.Name;
            newObj.HelpText = vModel.HelpText;
            newObj.AmountMultiplier = vModel.AmountMultiplier;
            newObj.Rarity = vModel.Rarity;
            newObj.PuissanceVariance = vModel.PuissanceVariance;
            newObj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            newObj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            newObj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);
            newObj.PopulationHardCap = vModel.PopulationHardCap;
            newObj.AmountMultiplier = vModel.AmountMultiplier;

            var newRace = BackingDataCache.Get<IRace>(vModel.Race);
            if (newRace != null)
                newObj.Race = newRace;
            else
                message += "Invalid race.";

            newObj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (String.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddFauna[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditFaunaViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();
            vModel.ValidRaces = BackingDataCache.GetAll<IRace>();

            var obj = BackingDataCache.Get<IFauna>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.Item2;
            vModel.ElevationRangeLow = obj.ElevationRange.Item1;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.Item2;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Item1;
            vModel.HumidityRangeHigh = obj.HumidityRange.Item2;
            vModel.HumidityRangeLow = obj.HumidityRange.Item1;
            vModel.PopulationHardCap = obj.PopulationHardCap;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.FemaleRatio = obj.FemaleRatio;
            vModel.Race = obj.Race.ID;
            vModel.OccursIn = obj.OccursIn.ToArray();

            return View("~/Views/GameAdmin/Fauna/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditFaunaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IFauna>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.HelpText = vModel.HelpText;
            obj.AmountMultiplier = vModel.AmountMultiplier;
            obj.Rarity = vModel.Rarity;
            obj.PuissanceVariance = vModel.PuissanceVariance;
            obj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);
            obj.PopulationHardCap = vModel.PopulationHardCap;
            obj.AmountMultiplier = vModel.AmountMultiplier;
            obj.FemaleRatio = vModel.FemaleRatio;

            var newRace = BackingDataCache.Get<IRace>(vModel.Race);
            if (newRace != null)
                obj.Race = newRace;
            else
                message += "Invalid race.";

            obj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (String.IsNullOrWhiteSpace(message))
            {
                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditFauna[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}