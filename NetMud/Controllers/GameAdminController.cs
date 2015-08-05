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
using System.IO;
using System.Text;
using System;



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

            dashboardModel.LivePlayers = LiveCache.GetAll<Player>().Count();

            dashboardModel.Inanimates = DataWrapper.GetAll<InanimateData>();
            dashboardModel.Rooms = DataWrapper.GetAll<RoomData>();
            dashboardModel.NPCs = DataWrapper.GetAll<NonPlayerCharacter>();
            dashboardModel.HelpFiles = ReferenceAccess.GetAll<Help>();
            dashboardModel.DimensionalModels = ReferenceAccess.GetAll<DimensionalModelData>();
            dashboardModel.LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens();

            return View(dashboardModel);
        }

        #region Dimensional Models
        public ActionResult ManageDimensionalModelData(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var vModel = new ManageDimensionalModelDataViewModel(ReferenceAccess.GetAll<DimensionalModelData>());
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

                var obj = ReferenceAccess.GetOne<DimensionalModelData>(ID);

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
                newObj.Name = newName;

                if (newObj.Create() == null)
                    message = "Error; Creation failed.";
                else
                {
                    LoggingUtility.LogAdminCommandUsage("*WEB* - AddDimensionalModelData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                    message = "Creation Successful.";
                }
            }
            catch(Exception ex)
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
            var vModel = new ManageHelpDataViewModel(ReferenceAccess.GetAll<Help>());
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

                var obj = ReferenceAccess.GetOne<Help>(ID);

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

            Help obj = ReferenceAccess.GetOne<Help>(id);

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

            Help obj = ReferenceAccess.GetOne<Help>(id);
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
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (vModel.InanimateContainerWeights.Count() <= inanimateIndex || vModel.InanimateContainerVolumes.Count() <= inanimateIndex)
                        break;

                    var currentWeight = vModel.InanimateContainerWeights[inanimateIndex];
                    var currentVolume = vModel.InanimateContainerVolumes[inanimateIndex];

                    newObj.InanimateContainers.Add(new EntityContainerData<IInanimate>(currentVolume, currentWeight, name));
                    inanimateIndex++;
                }
            }

            if (vModel.MobileContainerNames != null)
            {
                int mobileIndex = 0;
                foreach (var name in vModel.MobileContainerNames)
                {
                    if (string.IsNullOrWhiteSpace(name))
                        continue;

                    if (vModel.MobileContainerWeights.Count() <= mobileIndex || vModel.MobileContainerVolumes.Count() <= mobileIndex)
                        break;

                    var currentWeight = vModel.MobileContainerWeights[mobileIndex];
                    var currentVolume = vModel.MobileContainerVolumes[mobileIndex];

                    newObj.MobileContainers.Add(new EntityContainerData<IMobile>(currentVolume, currentWeight, name));
                    mobileIndex++;
                }
            }

            if (newObj.Create() == null)
                message = "Error; Creation failed.";
            else
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - AddInanimateData[" + newObj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Creation Successful.";
            }

            return RedirectToAction("ManageInanimateData", new { Message = message });
        }

        [HttpGet]
        public ActionResult EditInanimateData(int id)
        {
            string message = string.Empty;
            var vModel = new AddEditInanimateDataViewModel();
            vModel.authedUser = UserManager.FindById(User.Identity.GetUserId());

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
                        continue;

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
                        continue;

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

                    mobileIndex++;
                }
            }

            foreach (var container in obj.InanimateContainers.Where(ic => !vModel.InanimateContainerNames.Contains(ic.Name)))
                obj.InanimateContainers.Remove(container);

            if (obj.Save())
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditInanimateData[" + obj.ID.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

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
