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
    public class InanimateController : Controller
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

        public InanimateController()
        {
        }

        public InanimateController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageInanimateDataViewModel(BackingDataCache.GetAll<InanimateData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View("~/Views/GameAdmin/Inanimate/Index.cshtml", vModel);
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

                var obj = BackingDataCache.Get<InanimateData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveInanimate[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
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
            var vModel = new AddEditInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<InanimateData>();

            return View("~/Views/GameAdmin/Inanimate/Add.cshtml", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(AddEditInanimateDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new InanimateData();
            newObj.Name = vModel.NewName;

            if (vModel.InanimateContainerNames != null)
            {
                int inanimateIndex = 0;
                foreach (var name in vModel.InanimateContainerNames)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {

                        if (vModel.InanimateContainerWeights.Count() <= inanimateIndex || vModel.InanimateContainerVolumes.Count() <= inanimateIndex)
                            break;

                        var currentWeight = vModel.InanimateContainerWeights[inanimateIndex];
                        var currentVolume = vModel.InanimateContainerVolumes[inanimateIndex];

                        if (currentVolume > 0 && currentVolume > 0)
                            newObj.InanimateContainers.Add(new EntityContainerData<IInanimate>(currentVolume, currentWeight, name));
                    }

                    inanimateIndex++;
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

            var internalCompositions = new Dictionary<IInanimateData, short>();
            if (vModel.InternalCompositionIds != null)
            {
                int icIndex = 0;
                foreach (var id in vModel.InternalCompositionIds)
                {
                    if (id > 0)
                    {
                        if (vModel.InternalCompositionPercentages.Count() <= icIndex)
                            break;

                        var internalObj = BackingDataCache.Get<IInanimateData>(id);

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
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpGet]
        public ActionResult Edit(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = BackingDataCache.GetAll<Material>();
            vModel.ValidModels = BackingDataCache.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidInanimateDatas = BackingDataCache.GetAll<InanimateData>();

            var obj = BackingDataCache.Get<InanimateData>(id);

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

            return View("~/Views/GameAdmin/Inanimate/Edit.cshtml", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, AddEditInanimateDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = BackingDataCache.Get<InanimateData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("Index", new { Message = message });
            }

            obj.Name = vModel.NewName;

            if (vModel.InanimateContainerNames != null)
            {
                int inanimateIndex = 0;
                foreach (var name in vModel.InanimateContainerNames)
                {
                    if (!string.IsNullOrWhiteSpace(name))
                    {
                        if (vModel.InanimateContainerWeights.Count() <= inanimateIndex || vModel.InanimateContainerVolumes.Count() <= inanimateIndex)
                            break;

                        if (obj.InanimateContainers.Any(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase)))
                        {
                            var editIc = obj.InanimateContainers.Single(ic => ic.Name.Equals(name, System.StringComparison.InvariantCultureIgnoreCase));
                            editIc.CapacityVolume = vModel.InanimateContainerVolumes[inanimateIndex];
                            editIc.CapacityWeight = vModel.InanimateContainerWeights[inanimateIndex];
                        }
                        else
                        {
                            var currentWeight = vModel.InanimateContainerWeights[inanimateIndex];
                            var currentVolume = vModel.InanimateContainerVolumes[inanimateIndex];

                            if (currentVolume > 0 && currentWeight > 0)
                                obj.InanimateContainers.Add(new EntityContainerData<IInanimate>(currentVolume, currentWeight, name));
                        }
                    }

                    inanimateIndex++;
                }
            }

            foreach (var container in obj.InanimateContainers.Where(ic => !vModel.InanimateContainerNames.Contains(ic.Name)))
                obj.InanimateContainers.Remove(container);

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

            var internalCompositions = new Dictionary<IInanimateData, short>();
            if (vModel.InternalCompositionIds != null)
            {
                int icIndex = 0;
                foreach (var icId in vModel.InternalCompositionIds)
                {
                    if (icId > 0)
                    {
                        if (vModel.InternalCompositionPercentages.Count() <= icIndex)
                            break;

                        var internalObj = BackingDataCache.Get<InanimateData>(icId);

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
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
    }
}