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
    public class FloraController : Controller
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

        public FloraController()
        {
        }

        public FloraController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageFloraViewModel(BackingDataCache.GetAll<IFlora>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Flora/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<IFlora>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveFlora[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditFloraViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();

            return View("~/Views/GameAdmin/Flora/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Flora();
            newObj.Name = vModel.Name;
            newObj.HelpText = vModel.HelpText;
            newObj.SunlightPreference = vModel.SunlightPreference;
            newObj.Coniferous = vModel.Coniferous;
            newObj.AmountMultiplier = vModel.AmountMultiplier;
            newObj.Rarity = vModel.Rarity;
            newObj.PuissanceVariance = vModel.PuissanceVariance;
            newObj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            newObj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            newObj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newWood = BackingDataCache.Get<IMaterial>(vModel.Wood);
            if (newWood != null)
                newObj.Wood = newWood;

            var newFlower = BackingDataCache.Get<IInanimateData>(vModel.Flower);
            if (newFlower != null)
                newObj.Flower = newFlower;

            var newSeed = BackingDataCache.Get<IInanimateData>(vModel.Seed);
            if (newSeed != null)
                newObj.Seed = newSeed;

            var newLeaf = BackingDataCache.Get<IInanimateData>(vModel.Leaf);
            if (newLeaf != null)
                newObj.Leaf = newLeaf;

            var newFruit = BackingDataCache.Get<IInanimateData>(vModel.Fruit);
            if (newFruit != null)
                newObj.Fruit = newFruit;

            if (newWood == null && newFlower == null && newSeed == null && newLeaf == null && newFruit == null)
                message = "At least one of the parts of this plant must be valid.";

            newObj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (String.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddFlora[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditFloraViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<IMaterial>();
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<IInanimateData>();

            var obj = BackingDataCache.Get<IFlora>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText;
            vModel.SunlightPreference = obj.SunlightPreference;
            vModel.Coniferous = obj.Coniferous;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.Item2;
            vModel.ElevationRangeLow = obj.ElevationRange.Item1;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.Item2;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Item1;
            vModel.HumidityRangeHigh = obj.HumidityRange.Item2;
            vModel.HumidityRangeLow = obj.HumidityRange.Item1;
            vModel.Wood = obj.Wood.ID;
            vModel.Flower = obj.Flower.ID;
            vModel.Fruit = obj.Fruit.ID;
            vModel.Seed = obj.Seed.ID;
            vModel.Leaf = obj.Leaf.ID;
            vModel.OccursIn = obj.OccursIn.ToArray();

            return View("~/Views/GameAdmin/Flora/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<IFlora>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.HelpText = vModel.HelpText;
            obj.SunlightPreference = vModel.SunlightPreference;
            obj.Coniferous = vModel.Coniferous;
            obj.AmountMultiplier = vModel.AmountMultiplier;
            obj.Rarity = vModel.Rarity;
            obj.PuissanceVariance = vModel.PuissanceVariance;
            obj.ElevationRange = new Tuple<int, int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new Tuple<int, int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new Tuple<int, int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newWood = BackingDataCache.Get<IMaterial>(vModel.Wood);
            if (newWood != null)
                obj.Wood = newWood;

            var newFlower = BackingDataCache.Get<IInanimateData>(vModel.Flower);
            if (newFlower != null)
                obj.Flower = newFlower;

            var newSeed = BackingDataCache.Get<IInanimateData>(vModel.Seed);
            if (newSeed != null)
                obj.Seed = newSeed;

            var newLeaf = BackingDataCache.Get<IInanimateData>(vModel.Leaf);
            if (newLeaf != null)
                obj.Leaf = newLeaf;

            var newFruit = BackingDataCache.Get<IInanimateData>(vModel.Fruit);
            if (newFruit != null)
                obj.Fruit = newFruit;

            if(newWood == null)
                message = "Wood must be valid.";

            if (newFlower == null && newSeed == null && newLeaf == null && newFruit == null)
                message = "At least one of the parts of this plant must be valid.";

            obj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (String.IsNullOrWhiteSpace(message))
            {
                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditFlora[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}