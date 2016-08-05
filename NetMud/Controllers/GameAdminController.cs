using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity.EntityFramework;

using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.Data.EntityBackingData;
using NetMud.DataAccess;
using NetMud.Models.GameAdmin;
using NetMud.Data.Reference;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Entity;
using System.Text;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Behaviors.Actionable;
using NetMud.DataStructure.Behaviors.Automation;
using NetMud.Utility;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.System;
using WebSocketSharp.Server;

namespace NetMud.Controllers
{
    public class GameAdminController : Controller
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

        public GameAdminController()
        {
        }

        public GameAdminController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        //Also called Dashboard in most of the html
        public ActionResult Index()
        {
            var dashboardModel = new DashboardViewModel();
            dashboardModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            dashboardModel.Inanimates = DataWrapper.GetAll<IInanimateData>();
            dashboardModel.Rooms = DataWrapper.GetAll<IRoomData>();
            dashboardModel.NPCs = DataWrapper.GetAll<INonPlayerCharacter>();

            dashboardModel.HelpFiles = ReferenceWrapper.GetAll<IHelp>();
            dashboardModel.DimensionalModels = ReferenceWrapper.GetAll<IDimensionalModelData>();
            dashboardModel.Materials = ReferenceWrapper.GetAll<IMaterial>();
            dashboardModel.Races = ReferenceWrapper.GetAll<IRace>();
            dashboardModel.Zones = ReferenceWrapper.GetAll<IZone>();

            dashboardModel.LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens();
            dashboardModel.LivePlayers = LiveCache.GetAll<IPlayer>().Count();
            dashboardModel.LiveInanimates = LiveCache.GetAll<IInanimate>().Count();
            dashboardModel.LiveRooms = LiveCache.GetAll<IRoom>().Count();
            dashboardModel.LiveNPCs = LiveCache.GetAll<IIntelligence>().Count();

            dashboardModel.WebSocketServers = LiveCache.GetAll<WebSocketServer>();

            return View(dashboardModel);
        }

        #region Dimensional Models
        public ActionResult ManageDimensionalModelData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageDimensionalModelDataViewModel(ReferenceWrapper.GetAll<DimensionalModelData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveDimensionalModelData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ReferenceWrapper.GetOne<DimensionalModelData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveDimensionalModelData[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageDimensionalModelData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddDimensionalModelData()
        {
            var vModel = new AddEditDimensionalModelDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddDimensionalModelData(AddEditDimensionalModelDataViewModel vModel, HttpPostedFileBase modelFile)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            try
            {
                if (modelFile.ContentLength == 0)
                    message = "You must post a comma delimited file with the model in it.";

                byte[] bytes = new byte[modelFile.InputStream.Length];
                modelFile.InputStream.Read(bytes, 0, (int)modelFile.InputStream.Length);
                var fileContents = Encoding.UTF8.GetString(bytes);

                var newObj = new DimensionalModelData(fileContents);
                newObj.Name = vModel.NewName;
                newObj.ModelType = vModel.NewModelType;

                if (newObj.IsModelValid())
                {
                    if (newObj.Create() == null)
                        message = "Error; Creation failed.";
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddDimensionalModelData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Creation Successful.";
                    }
                }
                else
                    message = "Invalid model file; Model files must contain 11 planes of a tag name followed by 11 rows of 11 nodes.";
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
                message = "Error; Creation failed.";
            }

            return RedirectToAction("ManageDimensionalModelData", new { Message = message });
        }
        #endregion

        #region Help Files
        public ActionResult ManageHelpData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageHelpDataViewModel(ReferenceWrapper.GetAll<Help>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveHelpData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ReferenceWrapper.GetOne<Help>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveHelpFile[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageHelpData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddHelpData()
        {
            var vModel = new AddEditHelpDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddHelpData(string newName, string newHelpText)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Help();
            newObj.Name = newName;
            newObj.HelpText = newHelpText;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddHelpFile[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageHelpData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditHelpData(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditHelpDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            Help obj = ReferenceWrapper.GetOne<Help>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageHelpData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.NewHelpText = obj.HelpText;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditHelpData(string newName, string newHelpText, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Help obj = ReferenceWrapper.GetOne<Help>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageHelpData", new { Message = message });
            }

            obj.Name = newName;
            obj.HelpText = newHelpText;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditHelpFile[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageHelpData", new { Message = message });
        }
        #endregion

        #region NPCs
        public ActionResult ManageNPCData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageNPCDataViewModel(DataWrapper.GetAll<NonPlayerCharacter>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveNPCData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = DataWrapper.GetOne<NonPlayerCharacter>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveNPC[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageNPCData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddNPCData()
        {
            var vModel = new AddEditNPCDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidRaces = ReferenceWrapper.GetAll<Race>();

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNPCData(string newName, string newSurName, string newGender, long raceId)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new NonPlayerCharacter();
            newObj.Name = newName;
            newObj.SurName = newSurName;
            newObj.Gender = newGender;
            var race = ReferenceWrapper.GetOne<Race>(raceId);

            if (race != null)
                newObj.RaceData = race;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddNPCData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageNPCData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditNPCData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditNPCDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidRaces = ReferenceWrapper.GetAll<Race>();

            var obj = DataWrapper.GetOne<NonPlayerCharacter>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageNPCData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.NewGender = obj.Gender;
            vModel.NewSurName = obj.SurName;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditNPCData(string newName, string newSurName, string newGender, long raceId, int id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<NonPlayerCharacter>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageNPCData", new { Message = message });
            }

            obj.Name = newName;
            obj.SurName = newSurName;
            obj.Gender = newGender;
            var race = ReferenceWrapper.GetOne<Race>(raceId);

            if (race != null)
                obj.RaceData = race;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditNPCData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageNPCData", new { Message = message });
        }
        #endregion

        #region Inanimates
        public ActionResult ManageInanimateData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageInanimateDataViewModel(DataWrapper.GetAll<InanimateData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveInanimateData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = DataWrapper.GetOne<InanimateData>(ID);

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

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddInanimateData()
        {
            var vModel = new AddEditInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.ThreeD);
            vModel.ValidInanimateDatas = DataWrapper.GetAll<InanimateData>();

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddInanimateData(AddEditInanimateDataViewModel vModel)
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

                        var material = ReferenceWrapper.GetOne<Material>(vModel.ModelPartMaterials[nameIndex]);

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

                        var internalObj = DataWrapper.GetOne<InanimateData>(id);

                        if (internalObj != null && vModel.InternalCompositionPercentages[icIndex] > 0)
                            internalCompositions.Add(internalObj, vModel.InternalCompositionPercentages[icIndex]);
                    }

                    icIndex++;
                }
            }

            newObj.InternalComposition = internalCompositions;

            var dimModel = ReferenceWrapper.GetOne<DimensionalModelData>(vModel.DimensionalModelId);
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
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, vModel.DimensionalModelId, materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditInanimateData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.ThreeD);
            vModel.ValidInanimateDatas = DataWrapper.GetAll<InanimateData>();

            var obj = DataWrapper.GetOne<InanimateData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageInanimateData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.DimensionalModelId = obj.Model.ModelBackingData.ID;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.ModelDataObject = obj.Model;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditInanimateData(int id, AddEditInanimateDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<InanimateData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageInanimateData", new { Message = message });
            }

            obj.Name = vModel.NewName;

            if (vModel.InanimateContainerNames != null)
            {
                int inanimateIndex = 0;
                foreach (var name in vModel.InanimateContainerNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
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
                    if (string.IsNullOrWhiteSpace(name))
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

            foreach (var container in obj.InanimateContainers.Where(ic => !vModel.InanimateContainerNames.Contains(ic.Name)))
                obj.InanimateContainers.Remove(container);

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

                        var material = ReferenceWrapper.GetOne<Material>(vModel.ModelPartMaterials[nameIndex]);

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

                        var internalObj = DataWrapper.GetOne<InanimateData>(icId);

                        if (internalObj != null)
                            internalCompositions.Add(internalObj, vModel.InternalCompositionPercentages[icIndex]);
                    }

                    icIndex++;
                }
            }
            obj.InternalComposition = internalCompositions;

            var dimModel = ReferenceWrapper.GetOne<DimensionalModelData>(vModel.DimensionalModelId);
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
                obj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, vModel.DimensionalModelId, materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }
        #endregion

        #region Rooms
        public ActionResult ManageRoomData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageRoomDataViewModel(DataWrapper.GetAll<RoomData>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRoomData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = DataWrapper.GetOne<RoomData>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRoom[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageRoomData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddRoomData()
        {
            var vModel = new AddEditRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<IMaterial>();
            vModel.ValidZones = ReferenceWrapper.GetAll<IZone>();

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoomData(AddEditRoomDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new RoomData();
            newObj.Name = vModel.NewName;
            newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth);

            if (vModel.BorderMaterials != null)
            {
                int index = 0;
                foreach (var materialId in vModel.BorderMaterials)
                {
                    if (materialId > 0)
                    {
                        if (vModel.BorderNames.Count() <= index)
                            break;

                        var name = vModel.BorderNames[index];
                        var material = ReferenceWrapper.GetOne<IMaterial>(materialId);

                        if (material != null && !string.IsNullOrWhiteSpace(name) && !newObj.Borders.ContainsKey(name))
                            newObj.Borders.Add(name, material);
                    }

                    index++;
                }
            }

            var mediumId = vModel.Medium;
            var medium = ReferenceWrapper.GetOne<IMaterial>(mediumId);

            if (medium != null)
            {
                newObj.Medium = medium;

                var zoneId = vModel.Zone;
                var zone = ReferenceWrapper.GetOne<IZone>(zoneId);

                if (zone != null)
                {
                    newObj.ZoneAffiliation = zone;

                    if (newObj.Create() == null)
                        message = "Error; Creation failed.";
                    else
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Creation Successful.";
                    }
                }
                else
                    message = "You must include a valid Zone.";
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToAction("ManageRoomData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditRoomData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<IMaterial>();
            vModel.ValidZones = ReferenceWrapper.GetAll<IZone>();

            var obj = DataWrapper.GetOne<RoomData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRoomData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRoomData(AddEditRoomDataViewModel vModel, int id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<RoomData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRoomData", new { Message = message });
            }

            obj.Name = vModel.NewName;
            obj.Model.Height = vModel.DimensionalModelHeight;
            obj.Model.Length = vModel.DimensionalModelLength;
            obj.Model.Width = vModel.DimensionalModelWidth;

            obj.Borders = new Dictionary<string, IMaterial>();
            if (vModel.BorderMaterials != null)
            {
                int index = 0;
                foreach (var materialId in vModel.BorderMaterials)
                {
                    if (materialId > 0)
                    {
                        if (vModel.BorderNames.Count() <= index)
                            break;

                        var name = vModel.BorderNames[index];
                        var material = ReferenceWrapper.GetOne<IMaterial>(materialId);

                        if (material != null && !string.IsNullOrWhiteSpace(name) && !obj.Borders.ContainsKey(name))
                            obj.Borders.Add(name, material);
                    }

                    index++;
                }
            }

            var mediumId = vModel.Medium;
            var medium = ReferenceWrapper.GetOne<IMaterial>(mediumId);

            if (medium != null)
            {
                obj.Medium = medium;

                var zoneId = vModel.Zone;
                var zone = ReferenceWrapper.GetOne<IZone>(zoneId);

                if (zone != null)
                {
                    obj.ZoneAffiliation = zone;

                    if (obj.Save())
                    {
                        LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                        message = "Edit Successful.";
                    }
                    else                        
                        message = "Error; Edit failed.";
                }
                else
                    message = "You must include a valid Zone.";
            }
            else
                message = "You must include a valid Medium material.";

            return RedirectToAction("ManageRoomData", new { Message = message });
        }
        #endregion

        #region Pathways
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePathway(long ID, string authorize)
        {
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            string message = string.Empty;
            long roomId = -1;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = DataWrapper.GetOne<PathwayData>(ID);
                roomId = DataUtility.TryConvert<long>(obj.FromLocationID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemovePathway[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return View("AddEditPathway", vModel);
        }

        [HttpGet]
        public ActionResult AddPathway(long id)
        {
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidRooms = DataWrapper.GetAll<RoomData>().Where(rm => !rm.ID.Equals(id));

            return View("AddEditPathway", vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddPathway(AddEditPathwayDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new PathwayData();
            newObj.Name = vModel.NewName;
            newObj.AudibleStrength = vModel.AudibleStrength;
            newObj.AudibleToSurroundings = vModel.AudibleToSurroundings;
            newObj.DegreesFromNorth = vModel.DegreesFromNorth;
            newObj.FromLocationID = id.ToString();
            newObj.FromLocationType = "RoomData";
            newObj.MessageToActor = vModel.MessageToActor;
            newObj.MessageToDestination = vModel.MessageToDestination;
            newObj.MessageToOrigin = vModel.MessageToOrigin;
            newObj.ToLocationID = vModel.ToLocation.ID.ToString();
            newObj.ToLocationType = "RoomData";
            newObj.VisibleStrength = vModel.VisibleStrength;
            newObj.VisibleToSurroundings = vModel.VisibleToSurroundings;

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

                        var material = ReferenceWrapper.GetOne<Material>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null && !string.IsNullOrWhiteSpace(partName))
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = ReferenceWrapper.GetOne<DimensionalModelData>(vModel.DimensionalModelId);
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
                newObj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, vModel.DimensionalModelId, materialParts);

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddPathway[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }

            return View("AddEditPathway", vModel);
        }

        [HttpGet]
        public ActionResult EditPathway(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditPathwayDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>().Where(model => model.ModelType == DimensionalModelType.Flat);
            vModel.ValidRooms = DataWrapper.GetAll<RoomData>().Where(rm => !rm.ID.Equals(id));

            var obj = DataWrapper.GetOne<PathwayData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManagePathwayData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;

            vModel.AudibleStrength = obj.AudibleStrength;
            vModel.AudibleToSurroundings = obj.AudibleToSurroundings;
            vModel.DegreesFromNorth = obj.DegreesFromNorth;
            vModel.MessageToActor = obj.MessageToActor;
            vModel.MessageToDestination = obj.MessageToDestination;
            vModel.MessageToOrigin = obj.MessageToOrigin;
            vModel.ToLocation = DataWrapper.GetOne<RoomData>(DataUtility.TryConvert<long>(obj.ToLocationID));
            vModel.VisibleStrength = obj.VisibleStrength;
            vModel.VisibleToSurroundings = obj.VisibleToSurroundings;

            vModel.DimensionalModelId = obj.Model.ModelBackingData.ID;
            vModel.DimensionalModelHeight = obj.Model.Height;
            vModel.DimensionalModelLength = obj.Model.Length;
            vModel.DimensionalModelWidth = obj.Model.Width;
            vModel.ModelDataObject = obj.Model;

            return View("AddEditPathway", vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditPathway(long id, AddEditPathwayDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<PathwayData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return View("AddEditPathway", vModel);
            }

            obj.Name = vModel.NewName;
            obj.AudibleStrength = vModel.AudibleStrength;
            obj.AudibleToSurroundings = vModel.AudibleToSurroundings;
            obj.DegreesFromNorth = vModel.DegreesFromNorth;
            obj.FromLocationID = id.ToString();
            obj.FromLocationType = "RoomData";
            obj.MessageToActor = vModel.MessageToActor;
            obj.MessageToDestination = vModel.MessageToDestination;
            obj.MessageToOrigin = vModel.MessageToOrigin;
            obj.ToLocationID = vModel.ToLocation.ID.ToString();
            obj.ToLocationType = "RoomData";
            obj.VisibleStrength = vModel.VisibleStrength;
            obj.VisibleToSurroundings = vModel.VisibleToSurroundings;

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

                        var material = ReferenceWrapper.GetOne<Material>(vModel.ModelPartMaterials[nameIndex]);

                        if (material != null)
                            materialParts.Add(partName, material);
                    }

                    nameIndex++;
                }
            }

            var dimModel = ReferenceWrapper.GetOne<DimensionalModelData>(vModel.DimensionalModelId);
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
                obj.Model = new DimensionalModel(vModel.DimensionalModelHeight, vModel.DimensionalModelLength, vModel.DimensionalModelWidth, vModel.DimensionalModelId, materialParts);

                if (obj.Save())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - EditPathwayData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Edit Successful.";
                }
                else
                    message = "Error; Edit failed.";
            }

            //Don't return to the room editor, this is in a window
            return View("AddEditPathway", vModel);
        }
        #endregion

        #region Zones
        public ActionResult ManageZoneData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageZoneDataViewModel(ReferenceWrapper.GetAll<Zone>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveZoneData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ReferenceWrapper.GetOne<IZone>(ID);

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

            return RedirectToAction("ManageZoneData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddZoneData()
        {
            var vModel = new AddEditZoneDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddZoneData(AddEditZoneDataViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Zone();
            newObj.Name = vModel.Name;
            newObj.BaseElevation = vModel.BaseElevation;
            newObj.Claimable = vModel.Claimable;
            newObj.Owner = vModel.Owner;
            newObj.PressureCoefficient = vModel.PressureCoefficient;
            newObj.TemperatureCoefficient = vModel.TemperatureCoefficient;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddZone[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageZoneData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditZoneData(long id)
        {
            string message = string.Empty;
            var vModel = new AddEditZoneDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            Zone obj = ReferenceWrapper.GetOne<Zone>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageZoneData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.Name = obj.Name;
            vModel.BaseElevation = obj.BaseElevation;
            vModel.Claimable = obj.Claimable;
            vModel.Owner = obj.Owner;
            vModel.PressureCoefficient = obj.PressureCoefficient;
            vModel.TemperatureCoefficient = obj.TemperatureCoefficient;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditZoneData(AddEditZoneDataViewModel vModel, long id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Zone obj = ReferenceWrapper.GetOne<Zone>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageZoneData", new { Message = message });
            }

            obj.Name = vModel.Name;
            obj.BaseElevation = vModel.BaseElevation;
            obj.Claimable = vModel.Claimable;
            obj.Owner = vModel.Owner;
            obj.PressureCoefficient = vModel.PressureCoefficient;
            obj.TemperatureCoefficient = vModel.TemperatureCoefficient;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditZone[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageZoneData", new { Message = message });
        }
        #endregion

        #region Materials
        public ActionResult ManageMaterialData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageMaterialDataViewModel(ReferenceWrapper.GetAll<Material>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveMaterialData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ReferenceWrapper.GetOne<Material>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveMaterial[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageMaterialData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddMaterialData()
        {
            var vModel = new AddEditMaterialViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddMaterialData(AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Material();
            newObj.Name = vModel.NewName;
            newObj.Conductive = vModel.NewConductive;
            newObj.Density = vModel.NewDensity;
            newObj.Ductility = vModel.NewDuctility;
            newObj.Flammable = vModel.NewFlammable;
            newObj.GasPoint = vModel.NewGasPoint;
            newObj.Magnetic = vModel.NewMagnetic;
            newObj.Mallebility = vModel.NewMallebility;
            newObj.Porosity = vModel.NewPorosity;
            newObj.SolidPoint = vModel.NewSolidPoint;
            newObj.TemperatureRetention = vModel.NewTemperatureRetention;
            newObj.Viscosity = vModel.NewViscosity;

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
                        var material = ReferenceWrapper.GetOne<Material>(materialId);

                        if (material != null && currentValue > 0)
                            newObj.Composition.Add(material, currentValue);
                    }

                    compositionsIndex++;
                }
            }

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddMaterialData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageMaterialData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditMaterialData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditMaterialViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();

            var obj = ReferenceWrapper.GetOne<Material>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageMaterialData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.NewConductive = obj.Conductive;
            vModel.NewDensity = obj.Density;
            vModel.NewDuctility = obj.Ductility;
            vModel.NewFlammable = obj.Flammable;
            vModel.NewGasPoint = obj.GasPoint;
            vModel.NewMagnetic = obj.Magnetic;
            vModel.NewMallebility = obj.Mallebility;
            vModel.NewPorosity = obj.Porosity;
            vModel.NewSolidPoint = obj.SolidPoint;
            vModel.NewTemperatureRetention = obj.TemperatureRetention;
            vModel.NewViscosity = obj.Viscosity;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditMaterialData(int id, AddEditMaterialViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = ReferenceWrapper.GetOne<Material>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageMaterialData", new { Message = message });
            }

            obj.Name = vModel.NewName;
            obj.Conductive = vModel.NewConductive;
            obj.Density = vModel.NewDensity;
            obj.Ductility = vModel.NewDuctility;
            obj.Flammable = vModel.NewFlammable;
            obj.GasPoint = vModel.NewGasPoint;
            obj.Magnetic = vModel.NewMagnetic;
            obj.Mallebility = vModel.NewMallebility;
            obj.Porosity = vModel.NewPorosity;
            obj.SolidPoint = vModel.NewSolidPoint;
            obj.TemperatureRetention = vModel.NewTemperatureRetention;
            obj.Viscosity = vModel.NewViscosity;

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

                        var material = ReferenceWrapper.GetOne<Material>(materialId);
                        short currentValue = -1;

                        if (material != null)
                        {
                            if (obj.Composition.Any(ic => ic.Key.ID == materialId))
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

            foreach (var container in obj.Composition.Where(ic => !vModel.Compositions.Contains(ic.Key.ID)))
                obj.Composition.Remove(container);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditMaterialData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageMaterialData", new { Message = message });
        }
        #endregion

        #region Races
        public ActionResult ManageRaceData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageRaceDataViewModel(ReferenceWrapper.GetAll<Race>());
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemoveRaceData(long ID, string authorize)
        {
            string message = string.Empty;

            if (string.IsNullOrWhiteSpace(authorize) || !ID.ToString().Equals(authorize))
                message = "You must check the proper authorize radio button first.";
            else
            {
                var authedUser = UserManager.FindById(User.Identity.GetUserId());

                var obj = ReferenceWrapper.GetOne<Race>(ID);

                if (obj == null)
                    message = "That does not exist";
                else if (obj.Remove())
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - RemoveRace[" + ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Delete Successful.";
                }
                else
                    message = "Error; Removal failed.";
            }

            return RedirectToAction("ManageRaceData", new { Message = message });
        }

        [HttpGet]
        public ActionResult AddRaceData()
        {
            var vModel = new AddEditRaceViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidObjects = DataWrapper.GetAll<InanimateData>();
            vModel.ValidRooms = DataWrapper.GetAll<RoomData>();

            return View(vModel);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRaceData(AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new Race();
            newObj.Name = vModel.NewName;

            if (vModel.NewArmsID > 0 && vModel.NewArmsAmount > 0)
            {
                var arm = DataWrapper.GetOne<InanimateData>(vModel.NewArmsID);

                if (arm != null)
                    newObj.Arms = new Tuple<IInanimateData, short>(arm, vModel.NewArmsAmount);
            }

            if (vModel.NewLegsID > 0 && vModel.NewLegsAmount > 0)
            {
                var leg = DataWrapper.GetOne<InanimateData>(vModel.NewLegsID);

                if (leg != null)
                    newObj.Legs = new Tuple<IInanimateData, short>(leg, vModel.NewLegsAmount);
            }

            if (vModel.NewTorsoId > 0)
            {
                var torso = DataWrapper.GetOne<InanimateData>(vModel.NewTorsoId);

                if (torso != null)
                    newObj.Torso = torso;
            }

            if (vModel.NewHeadId > 0)
            {
                var head = DataWrapper.GetOne<InanimateData>(vModel.NewHeadId);

                if (head != null)
                    newObj.Head = head;
            }

            if (vModel.NewStartingLocationId > 0)
            {
                var room = DataWrapper.GetOne<RoomData>(vModel.NewStartingLocationId);

                if (room != null)
                    newObj.StartingLocation = room;
            }

            if (vModel.NewRecallLocationId > 0)
            {
                var room = DataWrapper.GetOne<RoomData>(vModel.NewRecallLocationId);

                if (room != null)
                    newObj.EmergencyLocation = room;
            }

            if (vModel.NewBloodId > 0)
            {
                var blood = ReferenceWrapper.GetOne<Material>(vModel.NewBloodId);

                if (blood != null)
                    newObj.SanguinaryMaterial = blood;
            }

            newObj.VisionRange = new Tuple<short, short>(vModel.NewVisionRangeLow, vModel.NewVisionRangeHigh);
            newObj.TemperatureTolerance = new Tuple<short, short>(vModel.NewTemperatureToleranceLow, vModel.NewTemperatureToleranceHigh);

            newObj.Breathes = (RespiratoryType)vModel.NewBreathes;
            newObj.DietaryNeeds = (DietType)vModel.NewDietaryNeeds;
            newObj.TeethType = (DamageType)vModel.NewTeethType;

            if (vModel.NewExtraPartsId != null)
            {
                int partIndex = 0;
                var bodyBits = new List<Tuple<IInanimateData, short, string>>();
                foreach (var id in vModel.NewExtraPartsId)
                {
                    if (id > 0)
                    {
                        if (vModel.NewExtraPartsAmount.Count() <= partIndex || vModel.NewExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.NewExtraPartsName[partIndex];
                        var currentAmount = vModel.NewExtraPartsAmount[partIndex];
                        var partObject = DataWrapper.GetOne<InanimateData>(id);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateData, short, string>(partObject, currentAmount, currentName));
                    }

                    partIndex++;
                }

                newObj.BodyParts = bodyBits;
            }

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRaceData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageRaceData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditRaceData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRaceViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());
            vModel.ValidMaterials = ReferenceWrapper.GetAll<Material>();
            vModel.ValidObjects = DataWrapper.GetAll<InanimateData>();
            vModel.ValidRooms = DataWrapper.GetAll<RoomData>();

            var obj = ReferenceWrapper.GetOne<Race>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRaceData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;
            vModel.NewArmsAmount = obj.Arms.Item2;
            vModel.NewArmsID = obj.Arms.Item1.ID;
            vModel.NewBloodId = obj.SanguinaryMaterial.ID;
            vModel.NewBreathes = (short)obj.Breathes;
            vModel.NewDietaryNeeds = (short)obj.DietaryNeeds;
            vModel.NewHeadId = obj.Head.ID;
            vModel.NewLegsAmount = obj.Legs.Item2;
            vModel.NewLegsID = obj.Legs.Item1.ID;
            vModel.NewRecallLocationId = obj.EmergencyLocation.ID;
            vModel.NewStartingLocationId = obj.StartingLocation.ID;
            vModel.NewTeethType = (short)obj.TeethType;
            vModel.NewTemperatureToleranceHigh = obj.TemperatureTolerance.Item2;
            vModel.NewTemperatureToleranceLow = obj.TemperatureTolerance.Item1;
            vModel.NewTorsoId = obj.Torso.ID;
            vModel.NewVisionRangeHigh = obj.VisionRange.Item2;
            vModel.NewVisionRangeLow = obj.VisionRange.Item1;

            vModel.NewExtraPartsAmount = obj.BodyParts.Select(bp => bp.Item2).ToArray();
            vModel.NewExtraPartsId = obj.BodyParts.Select(bp => bp.Item1.ID).ToArray(); ;
            vModel.NewExtraPartsName = obj.BodyParts.Select(bp => bp.Item3).ToArray(); ;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRaceData(int id, AddEditRaceViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = ReferenceWrapper.GetOne<Race>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRaceData", new { Message = message });
            }

            obj.Name = vModel.NewName;

            if (vModel.NewArmsID > 0 && vModel.NewArmsAmount > 0)
            {
                var arm = DataWrapper.GetOne<InanimateData>(vModel.NewArmsID);

                if (arm != null)
                    obj.Arms = new Tuple<IInanimateData, short>(arm, vModel.NewArmsAmount);
            }

            if (vModel.NewLegsID > 0 && vModel.NewLegsAmount > 0)
            {
                var leg = DataWrapper.GetOne<InanimateData>(vModel.NewLegsID);

                if (leg != null)
                    obj.Legs = new Tuple<IInanimateData, short>(leg, vModel.NewLegsAmount);
            }

            if (vModel.NewTorsoId > 0)
            {
                var torso = DataWrapper.GetOne<InanimateData>(vModel.NewTorsoId);

                if (torso != null)
                    obj.Torso = torso;
            }

            if (vModel.NewHeadId > 0)
            {
                var head = DataWrapper.GetOne<InanimateData>(vModel.NewHeadId);

                if (head != null)
                    obj.Head = head;
            }

            if (vModel.NewStartingLocationId > 0)
            {
                var room = DataWrapper.GetOne<RoomData>(vModel.NewStartingLocationId);

                if (room != null)
                    obj.StartingLocation = room;
            }

            if (vModel.NewRecallLocationId > 0)
            {
                var room = DataWrapper.GetOne<RoomData>(vModel.NewRecallLocationId);

                if (room != null)
                    obj.EmergencyLocation = room;
            }

            if (vModel.NewBloodId > 0)
            {
                var blood = ReferenceWrapper.GetOne<Material>(vModel.NewBloodId);

                if (blood != null)
                    obj.SanguinaryMaterial = blood;
            }

            obj.VisionRange = new Tuple<short, short>(vModel.NewVisionRangeLow, vModel.NewVisionRangeHigh);
            obj.TemperatureTolerance = new Tuple<short, short>(vModel.NewTemperatureToleranceLow, vModel.NewTemperatureToleranceHigh);

            obj.Breathes = (RespiratoryType)vModel.NewBreathes;
            obj.DietaryNeeds = (DietType)vModel.NewDietaryNeeds;
            obj.TeethType = (DamageType)vModel.NewTeethType;

            var bodyBits = new List<Tuple<IInanimateData, short, string>>();
            if (vModel.NewExtraPartsId != null)
            {
                int partIndex = 0;
                foreach (var partId in vModel.NewExtraPartsId)
                {
                    if (partId > 0)
                    {
                        if (vModel.NewExtraPartsAmount.Count() <= partIndex || vModel.NewExtraPartsName.Count() <= partIndex)
                            break;

                        var currentName = vModel.NewExtraPartsName[partIndex];
                        var currentAmount = vModel.NewExtraPartsAmount[partIndex];
                        var partObject = DataWrapper.GetOne<InanimateData>(partId);

                        if (partObject != null && currentAmount > 0 && !string.IsNullOrWhiteSpace(currentName))
                            bodyBits.Add(new Tuple<IInanimateData, short, string>(partObject, currentAmount, currentName));
                    }

                    partIndex++;
                }

                obj.BodyParts = bodyBits;
            }

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRaceData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageRaceData", new { Message = message });
        }
        #endregion

        #region Players/Users
        public ActionResult ManagePlayers(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            var vModel = new ManagePlayersViewModel(UserManager.Users);
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            vModel.CurrentPageNumber = CurrentPageNumber;
            vModel.ItemsPerPage = ItemsPerPage;
            vModel.SearchTerms = SearchTerms;

            vModel.ValidRoles = roleManager.Roles.ToList();

            return View(vModel);
        }

        [HttpPost]
        public JsonResult SelectCharacter(long CurrentlySelectedCharacter)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            if (authedUser != null && CurrentlySelectedCharacter > 0)
            {
                authedUser.GameAccount.CurrentlySelectedCharacter = CurrentlySelectedCharacter;
                UserManager.Update(authedUser);
            }

            return new JsonResult();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult RemovePlayer(long ID, string authorize)
        {
            string message = "Not Implimented";

            return RedirectToAction("ManagePlayers", new { Message = message });
        }
        #endregion

        #region Live Threads
        public ActionResult StopRunningProcess(string processName)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownLoop(processName, 600, "{0} seconds before " + processName + " is shutdown.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningProcess[" + processName + "]", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }

        public ActionResult StopRunningAllProcess()
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownAll(600, "{0} seconds before TOTAL WORLD SHUTDOWN.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region "Communication"
        public ActionResult StopRunningAllWebsockets()
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var servers = LiveCache.GetAllNonEntity<WebSocketServer>();

            foreach (var server in servers)
                server.Stop(WebSocketSharp.CloseStatusCode.Normal, "Shutting down");

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopALLWebSockets", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for all websockets.";

            return RedirectToAction("Index", new { Message = message });
        }

        public ActionResult StopWebSocket(int port)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var server = LiveCache.Get<WebSocketServer>("WebSocketServer_" + port.ToString());

            server.Stop(WebSocketSharp.CloseStatusCode.Normal, "Shutting down");

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopWebSocket[" + port.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        public ActionResult BackupWorld()
        {
            var hotBack = new HotBackup();

            hotBack.WriteLiveBackup();

            return RedirectToAction("Index", new { Message = "Backup Started" });
        }

        public ActionResult RestoreWorld()
        {
            var hotBack = new HotBackup();

            //TODO: Ensure we suspend EVERYTHING going on (fights, etc), add some sort of announcement globally and delay the entire thing on a timer

            //Write the players out first to maintain their positions
            hotBack.WritePlayers();

            //restore everything
            hotBack.RestoreLiveBackup();

            return RedirectToAction("Index", new { Message = "Restore Started" });
        }
        #endregion
    }
}
