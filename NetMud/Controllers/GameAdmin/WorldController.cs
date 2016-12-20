using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.EntityBackingData;
using NetMud.Data.LookupData;
using NetMud.Data.System;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models.Admin;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    public class WorldController : Controller
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

        public WorldController()
        {
        }

        public WorldController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageWorldDataViewModel(BackingDataCache.GetAll<WorldData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/World/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<WorldData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveWorld[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditWorldViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidWorldDatas = BackingDataCache.GetAll<WorldData>();

            return View("~/Views/GameAdmin/World/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditWorldViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new WorldData();
            newObj.Name = vModel.NewName;

            if (vModel.WorldContainerNames != null)
            {
                int WorldIndex = 0;
                foreach (var name in vModel.WorldContainerNames)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {

                        if (vModel.WorldContainerWeights.Count() <= WorldIndex || vModel.WorldContainerVolumes.Count() <= WorldIndex)
                            break;

                        var currentWeight = vModel.WorldContainerWeights[WorldIndex];
                        var currentVolume = vModel.WorldContainerVolumes[WorldIndex];

                        if (currentVolume > 0 && currentVolume > 0)
                            newObj.WorldContainers.Add(new EntityContainerData<IWorld>(currentVolume, currentWeight, name));
                    }

                    WorldIndex++;
                }
            }

            if (vModel.MobileContainerNames != null)
            {
                int mobileIndex = 0;
                foreach (var name in vModel.MobileContainerNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                    {
                        if (vModel.MobileContainerWeights.Count() <= mobileIndex || vModel.MobileContainerVolumes.Count() <= mobileIndex)
                            break;

                        var currentWeight = vModel.MobileContainerWeights[mobileIndex];
                        var currentVolume = vModel.MobileContainerVolumes[mobileIndex];

                        if (currentVolume > 0 && currentVolume > 0)
                            newObj.MobileContainers.Add(new EntityContainerData<IMobile>(currentVolume, currentWeight, name));
                    }

                    mobileIndex++;
                }
            }

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

            var internalCompositions = new Dictionary<IWorldData, short>();
            if (vModel.InternalCompositionIds != null)
            {
                int icIndex = 0;
                foreach (var id in vModel.InternalCompositionIds)
                {
                    if (id > 0)
                    {
                        if (vModel.InternalCompositionPercentages.Count() <= icIndex)
                            break;

                        var internalObj = BackingDataCache.Get<IWorldData>(id);

                        if (internalObj != null && vModel.InternalCompositionPercentages[icIndex] > 0)
                            internalCompositions.Add(internalObj, vModel.InternalCompositionPercentages[icIndex]);
                    }

                    icIndex++;
                }
            }

            newObj.InternalComposition = internalCompositions;

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
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth
                    , vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, vModel.DimensionalModelId, materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddWorldData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditWorldViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidWorldDatas = BackingDataCache.GetAll<WorldData>();

            var obj = BackingDataCache.Get<WorldData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.DimensionalModelId = obj.Model.ModelBackingData.ID;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.DimensionalModelVacuity = obj.Model.Vacuity;
            vModel.DimensionalModelCavitation = obj.Model.SurfaceCavitation;
            vModel.ModelDataObject = obj.Model;

            return View("~/Views/GameAdmin/World/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditWorldViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<WorldData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.NewName;

            if (vModel.WorldContainerNames != null)
            {
                int WorldIndex = 0;
                foreach (var name in vModel.WorldContainerNames)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (vModel.WorldContainerWeights.Count() <= WorldIndex || vModel.WorldContainerVolumes.Count() <= WorldIndex)
                            break;

                        if (obj.WorldContainers.Any(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var editIc = obj.WorldContainers.Single(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
                            editIc.CapacityVolume = vModel.WorldContainerVolumes[WorldIndex];
                            editIc.CapacityWeight = vModel.WorldContainerWeights[WorldIndex];
                        }
                        else
                        {
                            var currentWeight = vModel.WorldContainerWeights[WorldIndex];
                            var currentVolume = vModel.WorldContainerVolumes[WorldIndex];

                            if (currentVolume > 0 && currentWeight > 0)
                                obj.WorldContainers.Add(new EntityContainerData<IWorld>(currentVolume, currentWeight, name));
                        }
                    }

                    WorldIndex++;
                }
            }

            foreach (var container in obj.WorldContainers.Where(ic => !vModel.WorldContainerNames.Contains(ic.Name)))
                obj.WorldContainers.Remove(container);

            if (vModel.MobileContainerNames != null)
            {
                int mobileIndex = 0;
                foreach (var name in vModel.MobileContainerNames)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (vModel.MobileContainerWeights.Count() <= mobileIndex || vModel.MobileContainerVolumes.Count() <= mobileIndex)
                            break;

                        if (obj.MobileContainers.Any(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var editIc = obj.MobileContainers.Single(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
                            editIc.CapacityVolume = vModel.MobileContainerVolumes[mobileIndex];
                            editIc.CapacityWeight = vModel.MobileContainerWeights[mobileIndex];
                        }
                        else
                        {
                            var currentWeight = vModel.MobileContainerWeights[mobileIndex];
                            var currentVolume = vModel.MobileContainerVolumes[mobileIndex];

                            if (currentVolume > 0 && currentWeight > 0)
                                obj.MobileContainers.Add(new EntityContainerData<IMobile>(currentVolume, currentWeight, name));
                        }
                    }

                    mobileIndex++;
                }
            }

            foreach (var container in obj.MobileContainers.Where(ic => !vModel.MobileContainerNames.Contains(ic.Name)))
                obj.MobileContainers.Remove(container);

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

            var internalCompositions = new Dictionary<IWorldData, short>();
            if (vModel.InternalCompositionIds != null)
            {
                int icIndex = 0;
                foreach (var icId in vModel.InternalCompositionIds)
                {
                    if (icId > 0)
                    {
                        if (vModel.InternalCompositionPercentages.Count() <= icIndex)
                            break;

                        var internalObj = BackingDataCache.Get<WorldData>(icId);

                        if (internalObj != null)
                            internalCompositions.Add(internalObj, vModel.InternalCompositionPercentages[icIndex]);
                    }

                    icIndex++;
                }
            }
            obj.InternalComposition = internalCompositions;

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
                    vModel.DimensionalModelVacuity, vModel.DimensionalModelCavitation, vModel.DimensionalModelId, materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditWorldData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}