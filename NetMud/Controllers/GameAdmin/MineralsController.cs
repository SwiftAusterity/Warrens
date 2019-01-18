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
            var vModel = new ManageMineralsViewModel(TemplateCache.GetAll<IMineral>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Minerals/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Minerals/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IMineral>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMineral[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IMineral>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveMineral[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditMineralsViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidMinerals = TemplateCache.GetAll<IMineral>()
            };

            return View("~/Views/GameAdmin/Minerals/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditMineralsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Mineral
            {
                Name = vModel.Name,
                HelpText = vModel.HelpText,
                Solubility = vModel.Solubility,
                Fertility = vModel.Fertility,
                AmountMultiplier = vModel.AmountMultiplier,
                Rarity = vModel.Rarity,
                PuissanceVariance = vModel.PuissanceVariance,
                ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh),
                TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh),
                HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh)
            };

            var newRock = TemplateCache.Get<IMaterial>(vModel.Rock);
            if (newRock != null)
                newObj.Rock = newRock;
            else
                message += "Invalid rock material.";

            var newDirt = TemplateCache.Get<IMaterial>(vModel.Dirt);
            if (newDirt != null)
                newObj.Dirt = newDirt;
            else
                message += "Invalid dirt material.";

            newObj.OccursIn = new HashSet<Biome>(vModel.OccursIn);
            
            var newOres = new List<IMineral>();
            if (vModel.Ores != null)
            {
                foreach (var mineralId in vModel.Ores)
                {
                    if (mineralId >= 0)
                    {
                        var mineral = TemplateCache.Get<IMineral>(mineralId);

                        if (mineral != null)
                            newOres.Add(mineral);
                    }
                }

                if (newOres.Count > 0)
                    newObj.Ores = newOres;
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddMinerals[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditMineralsViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidMinerals = TemplateCache.GetAll<IMineral>().Where(m => m.Id != id),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>()
            };

            var obj = TemplateCache.Get<IMineral>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText.Value;
            vModel.Solubility = obj.Solubility;
            vModel.Fertility = obj.Fertility;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.High;
            vModel.ElevationRangeLow = obj.ElevationRange.Low;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.High;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Low;
            vModel.HumidityRangeHigh = obj.HumidityRange.High;
            vModel.HumidityRangeLow = obj.HumidityRange.Low;
            vModel.Rock = obj.Rock.Id;
            vModel.Dirt = obj.Dirt.Id;

            return View("~/Views/GameAdmin/Minerals/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditMineralsViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IMineral>(id);
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
            obj.ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);

            var newDirt = TemplateCache.Get<IMaterial>(vModel.Dirt);
            if (newDirt != null)
                obj.Dirt = newDirt;
            else
                message += "Invalid dirt material.";

            var newRock = TemplateCache.Get<IMaterial>(vModel.Rock);
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
                        var mineral = TemplateCache.Get<IMineral>(mineralId);

                        if (mineral != null)
                            newOres.Add(mineral);
                    }
                }

                if (newOres.Count > 0)
                    obj.Ores = newOres;
            }

            obj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (string.IsNullOrWhiteSpace(message))
            {
                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditMinerals[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}