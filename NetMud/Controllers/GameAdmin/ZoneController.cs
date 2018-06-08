using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Communication.Messaging;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class ZoneController : Controller
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

        public ZoneController()
        {
        }

        public ZoneController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageZoneDataViewModel(BackingDataCache.GetAll<IZoneData>())
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View("~/Views/GameAdmin/Zone/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<IZoneData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveZone[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditZoneDataViewModel()
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            return View("~/Views/GameAdmin/Zone/Add.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditZoneDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new ZoneData
            {
                Name = vModel.Name,
                BaseElevation = vModel.BaseElevation,
                PressureCoefficient = vModel.PressureCoefficient,
                TemperatureCoefficient = vModel.TemperatureCoefficient
            };

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddZone[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(long id)
        {
            string message = string.Empty;

            IZoneData obj = BackingDataCache.Get<IZoneData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            var locales = BackingDataCache.GetAll<ILocaleData>().Where(locale => locale.ParentLocation.Equals(obj));

            var vModel = new AddEditZoneDataViewModel(locales)
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                DataObject = obj,
                Name = obj.Name,
                BaseElevation = obj.BaseElevation,
                PressureCoefficient = obj.PressureCoefficient,
                TemperatureCoefficient = obj.TemperatureCoefficient
            };

            return View("~/Views/GameAdmin/Zone/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AddEditZoneDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            IZoneData obj = BackingDataCache.Get<IZoneData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BaseElevation = vModel.BaseElevation;
            obj.PressureCoefficient = vModel.PressureCoefficient;
            obj.TemperatureCoefficient = vModel.TemperatureCoefficient;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditZone[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddEditLocalePath(long id, long localeId)
        {
            var locale = BackingDataCache.Get<ILocaleData>(localeId);

            if(locale == null)
            {
                return RedirectToAction("Edit", new { Message = "Locale has no rooms", id });
            }

            var validRooms = BackingDataCache.GetAll<IRoomData>().Where(rm => rm.ParentLocation.Equals(locale));

            if(validRooms.Count() == 0)
            {
                return RedirectToAction("Edit", new { Message = "Locale has no rooms", id });
            }

            var origin = BackingDataCache.Get<IZoneData>(id);

            var existingPathway = origin.GetLocalePathways().FirstOrDefault(path => ((IRoomData)path.Destination).ParentLocation.Equals(locale));

            var vModel = new AddEditZonePathwayDataViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                ValidMaterials = BackingDataCache.GetAll<IMaterial>(),
                ValidModels = BackingDataCache.GetAll<IDimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat),
                ValidRooms = validRooms,

                Origin = origin,
                OriginID = id,

                DestinationID = -1
            };

            if(existingPathway != null)
            {
                vModel.Name = existingPathway.Name;
                vModel.Destination = (IRoomData)existingPathway.Destination;
                vModel.DestinationID = existingPathway.Destination.Id;

                vModel.DimensionalModelId = existingPathway.Model.ModelBackingData.Id;
                vModel.DimensionalModelHeight = existingPathway.Model.Height;
                vModel.DimensionalModelLength = existingPathway.Model.Length;
                vModel.DimensionalModelWidth = existingPathway.Model.Width;
                vModel.DimensionalModelVacuity = existingPathway.Model.Vacuity;
                vModel.DimensionalModelCavitation = existingPathway.Model.SurfaceCavitation;
                vModel.ModelDataObject = existingPathway.Model;

                vModel.DataObject = existingPathway;
            }

            return View("~/Views/GameAdmin/Zone/AddEditLocalePath.cshtml", vModel);
        }

        [HttpPost]
        public ActionResult AddLocalePathway(long id, AddEditZonePathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData
            {
                Name = vModel.Name,
                DegreesFromNorth = -1,
                Origin = BackingDataCache.Get<IZoneData>(vModel.OriginID),
                Destination = BackingDataCache.Get<IRoomData>(vModel.DestinationID),
            };

            var materialParts = new Dictionary<string, IMaterial>();
            if (vModel.ModelPartNames != null)
            {
                int nameIndex = 0;
                foreach (var partName in vModel.ModelPartNames)
                {
                    if (!string.IsNullOrWhiteSpace(partName))
                    {
                        if (vModel.ModelPartMaterials.Count() <= nameIndex)
                            break;

                        var material = BackingDataCache.Get<IMaterial>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null && !string.IsNullOrWhiteSpace(partName))
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = BackingDataCache.Get<IDimensionalModelData>(vModel.DimensionalModelId);
            bool validData = true;

            if (dimModel == null)
            {
                message = "Choose a valid dimensional model.";
                validData = false;
            }

            if (dimModel.ModelPlanes.Any(plane => !materialParts.ContainsKey(plane.TagName)))
            {
                message = "You need to choose a material for each Dimensional Model planar section. (" + string.Join(",", dimModel.ModelPlanes.Select(plane => plane.TagName)) + ")";
                validData = false;
            }

            if (validData)
            {
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, new BackingDataCacheKey(dimModel), materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }

            return RedirectToAction("Edit", new { Message = message, id });
        }

        [HttpPost]
        public ActionResult EditLocalePathway(long id, AddEditZonePathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidRooms = BackingDataCache.GetAll<IRoomData>();

            var obj = BackingDataCache.Get<IPathwayData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Edit", new { Message = message, id });
            }

            obj.Name = vModel.Name;
            obj.Destination = BackingDataCache.Get<IRoomData>(vModel.DestinationID);

            var materialParts = new Dictionary<string, IMaterial>();
            if (vModel.ModelPartNames != null)
            {
                int nameIndex = 0;
                foreach (var partName in vModel.ModelPartNames)
                {
                    if (!string.IsNullOrWhiteSpace(partName))
                    {
                        if (vModel.ModelPartMaterials.Count() <= nameIndex)
                            break;

                        var material = BackingDataCache.Get<Material>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null)
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = BackingDataCache.Get<DimensionalModelData>(vModel.DimensionalModelId);
            bool validData = true;

            if (dimModel == null)
            {
                message = "Choose a valid dimensional model.";
                validData = false;
            }

            if (dimModel.ModelPlanes.Any(plane => !materialParts.ContainsKey(plane.TagName)))
            {
                message = "You need to choose a material for each Dimensional Model planar section. (" + string.Join(",", dimModel.ModelPlanes.Select(plane => plane.TagName)) + ")";
                validData = false;
            }

            if (validData)
            {
                obj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth,
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, new BackingDataCacheKey(dimModel), materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayData[" + obj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
                else
                    message = "Error; Edit failed.";
            }


            return RedirectToAction("Edit", new { Message = message, id });
        }
    }
}