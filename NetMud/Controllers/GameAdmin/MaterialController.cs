﻿using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.LookupData;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Behaviors.System;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
    public class MaterialController : Controller
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

        public MaterialController()
        {
        }

        public MaterialController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageMaterialDataViewModel(BackingDataCache.GetAll<Material>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Material/Index.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Route(@"GameAdmin/Material/Remove/{removeId?}/{authorizeRemove?}/{unapproveId?}/{authorizeUnapprove?}")]
        public ActionResult Remove(long removeId = -1, string authorizeRemove = "", long unapproveId = -1, string authorizeUnapprove = "")
        {
            string message = string.Empty;

            if (!string.IsNullOrWhiteSpace(authorizeRemove) && removeId.ToString().Equals(authorizeRemove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IMaterial>(removeId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove(authedUser.GameAccount, authedUser.GetStaffRank(User)))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMaterial[" + removeId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }
            else if (!string.IsNullOrWhiteSpace(authorizeUnapprove) && unapproveId.ToString().Equals(authorizeUnapprove))
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = BackingDataCache.Get<IMaterial>(unapproveId);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.ChangeApprovalStatus(authedUser.GameAccount, authedUser.GetStaffRank(User), ApprovalState.Returned))
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - UnapproveMaterial[" + unapproveId.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditMaterialViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<IMaterial>()
            };

            return View("~/Views/GameAdmin/Material/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Material
            {
                Name = vModel.Name,
                Conductive = vModel.Conductive,
                Density = vModel.Density,
                Ductility = vModel.Ductility,
                Flammable = vModel.Flammable,
                GasPoint = vModel.GasPoint,
                Magnetic = vModel.Magnetic,
                Mallebility = vModel.Mallebility,
                Porosity = vModel.Porosity,
                SolidPoint = vModel.SolidPoint,
                TemperatureRetention = vModel.TemperatureRetention,
                Viscosity = vModel.Viscosity,
                HelpText = vModel.HelpBody
            };

            if (vModel.Resistances != null)
            {
                int resistancesIndex = 0;
                foreach (var type in vModel.Resistances)
                {
                    if (type > 0)
                    {
                        if (vModel.ResistanceValues.Count() <= resistancesIndex)
                            break;

                        var currentValue = vModel.ResistanceValues[resistancesIndex];

                        if (currentValue > 0)
                            newObj.Resistance.Add((DamageType)type, currentValue);
                    }

                    resistancesIndex++;
                }
            }

            if (vModel.Compositions != null)
            {
                int compositionsIndex = 0;
                foreach (var materialId in vModel.Compositions)
                {
                    if (materialId > 0)
                    {
                        if (vModel.CompositionPercentages.Count() <= compositionsIndex)
                            break;

                        var currentValue = vModel.CompositionPercentages[compositionsIndex];
                        var material = BackingDataCache.Get<Material>(materialId);

                        if (material != null && currentValue > 0)
                            newObj.Composition.Add(material, currentValue);
                    }

                    compositionsIndex++;
                }
            }

            if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddMaterialData[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditMaterialViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                ValidMaterials = BackingDataCache.GetAll<Material>()
            };

            var obj = BackingDataCache.Get<Material>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.Conductive = obj.Conductive;
            vModel.Density = obj.Density;
            vModel.Ductility = obj.Ductility;
            vModel.Flammable = obj.Flammable;
            vModel.GasPoint = obj.GasPoint;
            vModel.Magnetic = obj.Magnetic;
            vModel.Mallebility = obj.Mallebility;
            vModel.Porosity = obj.Porosity;
            vModel.SolidPoint = obj.SolidPoint;
            vModel.TemperatureRetention = obj.TemperatureRetention;
            vModel.Viscosity = obj.Viscosity;
            vModel.HelpBody = obj.HelpText;

            return View("~/Views/GameAdmin/Material/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<Material>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.Conductive = vModel.Conductive;
            obj.Density = vModel.Density;
            obj.Ductility = vModel.Ductility;
            obj.Flammable = vModel.Flammable;
            obj.GasPoint = vModel.GasPoint;
            obj.Magnetic = vModel.Magnetic;
            obj.Mallebility = vModel.Mallebility;
            obj.Porosity = vModel.Porosity;
            obj.SolidPoint = vModel.SolidPoint;
            obj.TemperatureRetention = vModel.TemperatureRetention;
            obj.Viscosity = vModel.Viscosity;
            obj.HelpText = vModel.HelpBody;

            if (vModel.Resistances != null)
            {
                int resistancesIndex = 0;
                foreach (var type in vModel.Resistances)
                {
                    if (type > 0)
                    {
                        if (vModel.ResistanceValues.Count() <= resistancesIndex)
                            break;

                        if (obj.Resistance.Any(ic => (short)ic.Key == type))
                        {
                            obj.Resistance.Remove((DamageType)type);
                            var currentValue = vModel.ResistanceValues[resistancesIndex];

                            obj.Resistance.Add((DamageType)type, currentValue);
                        }
                        else
                        {
                            var currentValue = vModel.ResistanceValues[resistancesIndex];

                            obj.Resistance.Add((DamageType)type, currentValue);
                        }
                    }

                    resistancesIndex++;
                }
            }

            foreach (var container in obj.Resistance.Where(ic => !vModel.Resistances.Contains((short)ic.Key)))
                obj.Resistance.Remove(container);

            if (vModel.Compositions != null)
            {
                int compositionsIndex = 0;
                foreach (var materialId in vModel.Compositions)
                {
                    if (materialId > 0)
                    {
                        if (vModel.CompositionPercentages.Count() <= compositionsIndex)
                            break;

                        var material = BackingDataCache.Get<Material>(materialId);
                        short currentValue = -1;

                        if (material != null)
                        {
                            if (obj.Composition.Any(ic => ic.Key.Id == materialId))
                            {

                                obj.Composition.Remove(material);
                                currentValue = vModel.CompositionPercentages[compositionsIndex];
                            }
                            else
                                currentValue = vModel.CompositionPercentages[compositionsIndex];

                            obj.Composition.Add(material, currentValue);
                        }
                    }

                    compositionsIndex++;
                }
            }

            foreach (var container in obj.Composition.Where(ic => !vModel.Compositions.Contains(ic.Key.Id)))
                obj.Composition.Remove(container);

            if (obj.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditMaterialData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
    }
}