﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.Zone;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var vModel = new ManageFaunaViewModel(TemplateCache.GetAll<IFauna>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Fauna/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Fauna/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IFauna>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveFauna[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = TemplateCache.Get<IFauna>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveFauna[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditFaunaViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidRaces = TemplateCache.GetAll<IRace>()
            };

            return View("~/Views/GameAdmin/Fauna/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditFaunaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Fauna
            {
                Name = vModel.Name,
                HelpText = vModel.HelpText,
                AmountMultiplier = vModel.AmountMultiplier,
                Rarity = vModel.Rarity,
                PuissanceVariance = vModel.PuissanceVariance,
                ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh),
                TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh),
                HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh),
                PopulationHardCap = vModel.PopulationHardCap,
                FemaleRatio = vModel.FemaleRatio
            };
            newObj.AmountMultiplier = vModel.AmountMultiplier;

            var newRace = TemplateCache.Get<IRace>(vModel.Race);
            if (newRace != null)
                newObj.Race = newRace;
            else
                message += "Invalid race.";

            newObj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (string.IsNullOrWhiteSpace(message))
            {
                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddFauna[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditFaunaViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = TemplateCache.GetAll<IMaterial>(),
                ValidInanimateTemplates = TemplateCache.GetAll<IInanimateTemplate>(),
                ValidRaces = TemplateCache.GetAll<IRace>()
            };

            var obj = TemplateCache.Get<IFauna>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.HelpText = obj.HelpText.Value;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.Rarity = obj.Rarity;
            vModel.PuissanceVariance = obj.PuissanceVariance;
            vModel.ElevationRangeHigh = obj.ElevationRange.High;
            vModel.ElevationRangeLow = obj.ElevationRange.Low;
            vModel.TemperatureRangeHigh = obj.TemperatureRange.High;
            vModel.TemperatureRangeLow = obj.TemperatureRange.Low;
            vModel.HumidityRangeHigh = obj.HumidityRange.High;
            vModel.HumidityRangeLow = obj.HumidityRange.Low;
            vModel.PopulationHardCap = obj.PopulationHardCap;
            vModel.AmountMultiplier = obj.AmountMultiplier;
            vModel.FemaleRatio = obj.FemaleRatio;
            vModel.Race = obj.Race.Id;
            vModel.OccursIn = obj.OccursIn.ToArray();

            return View("~/Views/GameAdmin/Fauna/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(long id, AddEditFaunaViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = TemplateCache.Get<IFauna>(id);
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
            obj.ElevationRange = new ValueRange<int>(vModel.ElevationRangeLow, vModel.ElevationRangeHigh);
            obj.TemperatureRange = new ValueRange<int>(vModel.TemperatureRangeLow, vModel.TemperatureRangeHigh);
            obj.HumidityRange = new ValueRange<int>(vModel.HumidityRangeLow, vModel.HumidityRangeHigh);
            obj.PopulationHardCap = vModel.PopulationHardCap;
            obj.AmountMultiplier = vModel.AmountMultiplier;
            obj.FemaleRatio = vModel.FemaleRatio;

            var newRace = TemplateCache.Get<IRace>(vModel.Race);
            if (newRace != null)
                obj.Race = newRace;
            else
                message += "Invalid race.";

            obj.OccursIn = new HashSet<Biome>(vModel.OccursIn);

            if (string.IsNullOrWhiteSpace(message))
            {
                if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditFauna[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}