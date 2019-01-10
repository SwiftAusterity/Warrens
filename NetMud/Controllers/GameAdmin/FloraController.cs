using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var vModel = new ManageFloraViewModel(TemplateCache.GetAll<IFlora>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Flora/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Flora/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IFlora>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveFlora[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IFlora>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveFlora[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Unapproval Successful.";
                }
                else
                    message = "Error; Unapproval failed.";
            }
            else
                message = "You must check the proper remove or unapprove authorization radio button first.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Add()
        {
            var vModel = new AddEditFloraViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>()
            };

            return View("~/Views/GameAdmin/Flora/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Flora
            {
                Name = vModel.Name,
                HelpText = vModel.HelpText,
                SunlightPreference = vModel.SunlightPreference,
                Coniferous = vModel.Coniferous,
                AmountMultiplier = vModel.AmountMultiplier,
                Rarity = vModel.Rarity,
                PuissanceVariance = vModel.PuissanceVariance,
                ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh),
                TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh),
                HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh)
            };

            var newWood = TemplateCache.Get<IMaterial>(vModel.Wood);
            if (newWood != null)
                newObj.Wood = newWood;

            var newFlower = TemplateCache.Get<IInanimateTemplate>(vModel.Flower);
            if (newFlower != null)
                newObj.Flower = newFlower;

            var newSeed = TemplateCache.Get<IInanimateTemplate>(vModel.Seed);
            if (newSeed != null)
                newObj.Seed = newSeed;

            var newLeaf = TemplateCache.Get<IInanimateTemplate>(vModel.Leaf);
            if (newLeaf != null)
                newObj.Leaf = newLeaf;

            var newFruit = TemplateCache.Get<IInanimateTemplate>(vModel.Fruit);
            if (newFruit != null)
                newObj.Fruit = newFruit;

            if (newWood == null && newFlower == null && newSeed == null && newLeaf == null && newFruit == null)
                message = "At least one of the parts of this plant must be valid.";

            newObj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (string.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddFlora[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditFloraViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>()
            };

            var obj = TemplateCache.Get<IFlora>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText.Value;
            vModel.SunlightPreference = obj.SunlightPreference;
            vModel.Coniferous = obj.Coniferous;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.High;
            vModel.ElevationRangeLow = obj.ElevationRange.Low;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.High;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Low;
            vModel.HumidityRangeHigh = obj.HumidityRange.High;
            vModel.HumidityRangeLow = obj.HumidityRange.Low;
            vModel.Wood = obj.Wood.Id;
            vModel.Flower = obj.Flower.Id;
            vModel.Fruit = obj.Fruit.Id;
            vModel.Seed = obj.Seed.Id;
            vModel.Leaf = obj.Leaf.Id;
            vModel.OccursIn = obj.OccursIn.ToArray();

            return View("~/Views/GameAdmin/Flora/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditFloraViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IFlora>(id);
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
            obj.ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newWood = TemplateCache.Get<IMaterial>(vModel.Wood);
            if (newWood != null)
                obj.Wood = newWood;

            var newFlower = TemplateCache.Get<IInanimateTemplate>(vModel.Flower);
            if (newFlower != null)
                obj.Flower = newFlower;

            var newSeed = TemplateCache.Get<IInanimateTemplate>(vModel.Seed);
            if (newSeed != null)
                obj.Seed = newSeed;

            var newLeaf = TemplateCache.Get<IInanimateTemplate>(vModel.Leaf);
            if (newLeaf != null)
                obj.Leaf = newLeaf;

            var newFruit = TemplateCache.Get<IInanimateTemplate>(vModel.Fruit);
            if (newFruit != null)
                obj.Fruit = newFruit;

            if(newWood == null)
                message = "Wood must be valid.";

            if (newFlower == null && newSeed == null && newLeaf == null && newFruit == null)
                message = "At least one of the parts of this plant must be valid.";

            obj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (string.IsNullOrWhiteSpace(message))
            {
                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditFlora[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}