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
using NetMud.Data.Game;
using NetMud.DataAccess;
using NetMud.Models.GameAdmin;
using NetMud.Models;
using NetMud.Data.Reference;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Data.System;
using NetMud.DataStructure.Base.Entity;
using System.Text;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;

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

            dashboardModel.Inanimates = DataWrapper.GetAll<InanimateData>();
            dashboardModel.Rooms = DataWrapper.GetAll<RoomData>();
            dashboardModel.NPCs = DataWrapper.GetAll<NonPlayerCharacter>();

            dashboardModel.HelpFiles = ReferenceWrapper.GetAll<Help>();
            dashboardModel.DimensionalModels = ReferenceWrapper.GetAll<DimensionalModelData>();
            dashboardModel.Materials = ReferenceWrapper.GetAll<Material>();

            dashboardModel.LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens();
            dashboardModel.LivePlayers = LiveCache.GetAll<Player>().Count();
            dashboardModel.LiveInanimates = LiveCache.GetAll<Inanimate>().Count();
            dashboardModel.LiveRooms = LiveCache.GetAll<Room>().Count();
            dashboardModel.LiveNPCs = LiveCache.GetAll<Intelligence>().Count();

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
        public ActionResult AddDimensionalModelData(string newName, HttpPostedFileBase modelFile)
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

                if (newObj.IsModelValid())
                {
                    newObj.Name = newName;

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

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddNPCData(string newName, string newSurName, string newGender)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new NonPlayerCharacter();
            newObj.Name = newName;
            newObj.SurName = newSurName;
            newObj.Gender = newGender;

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
        public ActionResult EditNPCData(string newName, string newSurName, string newGender, int id)
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
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>();
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
                foreach (var id in vModel.InternalCompositionIds)
                {
                    if (id > 0)
                    {
                        if (vModel.InternalCompositionPercentages.Count() <= icIndex)
                            break;

                        var internalObj = DataWrapper.GetOne<InanimateData>(id);

                        if (internalObj != null)
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
            vModel.ValidModels = ReferenceWrapper.GetAll<DimensionalModelData>();
            vModel.ValidInanimateDatas = DataWrapper.GetAll<InanimateData>();

            var obj = DataWrapper.GetOne<InanimateData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageInanimateData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;

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

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddRoomData(string newName)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var newObj = new RoomData();
            newObj.Name = newName;

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddRoomData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageRoomData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditRoomData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditRoomDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<RoomData>(id);

            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRoomData", new { Message = message });
            }

            vModel.DataObject = obj;
            vModel.NewName = obj.Name;

            return View(vModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult EditRoomData(string newName, int id)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var obj = DataWrapper.GetOne<RoomData>(id);
            if (obj == null)
            {
                message = "That does not exist";
                return RedirectToAction("ManageRoomData", new { Message = message });
            }

            obj.Name = newName;

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditRoomData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("ManageRoomData", new { Message = message });
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

        #region Players/Users
        public ActionResult ManagePlayers(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));

            var vModel = new ManagePlayersViewModel(UserManager.Users.ToList());
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

            Processor.ShutdownAll(600, "{0} econds before TOTAL WORLD SHUTDOWN.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        public ActionResult BackupWorld()
        {
            var hotBack = new HotBackup(HostingEnvironment.MapPath("/HotBackup/"));

            hotBack.WriteLiveBackup();

            return RedirectToAction("Index", new { Message = "Backup Started" });
        }

        public ActionResult RestoreWorld()
        {
            var hotBack = new HotBackup(HostingEnvironment.MapPath("/HotBackup/"));

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
