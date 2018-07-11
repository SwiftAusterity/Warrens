using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Entity;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Base.World;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.Admin;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers.GameAdmin
{
    [Authorize(Roles = "Admin,Builder")]
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
            var dashboardModel = new DashboardViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                Inanimates = BackingDataCache.GetAll<IInanimateData>(),
                Rooms = BackingDataCache.GetAll<IRoomData>(),
                NPCs = BackingDataCache.GetAll<INonPlayerCharacter>(),
                Zones = BackingDataCache.GetAll<IZoneData>(),
                Locales = BackingDataCache.GetAll<ILocaleData>(),
                Worlds = BackingDataCache.GetAll<IGaiaData>(),

                HelpFiles = BackingDataCache.GetAll<IHelp>(),
                DimensionalModels = BackingDataCache.GetAll<IDimensionalModelData>(),
                Materials = BackingDataCache.GetAll<IMaterial>(),
                Races = BackingDataCache.GetAll<IRace>(),
                Constants = BackingDataCache.GetAll<IConstants>(),
                Fauna = BackingDataCache.GetAll<IFauna>(),
                Flora = BackingDataCache.GetAll<IFlora>(),
                Minerals = BackingDataCache.GetAll<IMineral>(),
                UIModules = BackingDataCache.GetAll<IUIModule>(),
                Celestials = BackingDataCache.GetAll<ICelestial>(),
                Journals = BackingDataCache.GetAll<IJournalEntry>(),

                DictionaryWords = ConfigDataCache.GetAll<IDictata>(),
                Languages = ConfigDataCache.GetAll<ILanguage>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LivePlayers = LiveCache.GetAll<IPlayer>().Count(),
                LiveInanimates = LiveCache.GetAll<IInanimate>().Count(),
                LiveRooms = LiveCache.GetAll<IRoom>().Count(),
                LiveNPCs = LiveCache.GetAll<IIntelligence>().Count(),
                LiveLocales = LiveCache.GetAll<ILocale>().Count(),
                LiveZones = LiveCache.GetAll<IZone>().Count(),
                LiveWorlds = LiveCache.GetAll<IGaia>().Count(),
            };

            return View(dashboardModel);
        }

        public ActionResult ModalErrorOrClose(string Message = "")
        {
            return View("~/Views/GameAdmin/ModalErrorOrClose.cshtml", "_chromelessLayout", Message);
        }

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

        #region Running Data
        public ActionResult BackupWorld()
        {
            var hotBack = new HotBackup();

            hotBack.WriteLiveBackup();
            BackingData.WriteFullBackup();

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

        #region "Global Config"
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public ActionResult GlobalConfig()
        {
            var globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            var vModel = new GlobalConfigViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),
                DataObject = globalConfig,
                WebsocketPortalActive = globalConfig.WebsocketPortalActive
            };

            return View(vModel);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GlobalConfig(GlobalConfigViewModel vModel)
        {
            string message = string.Empty;
            var authedUser = UserManager.FindById(User.Identity.GetUserId());
            var globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            globalConfig.WebsocketPortalActive = vModel.WebsocketPortalActive;

            if (globalConfig.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditGlobalConfig[" + globalConfig.UniqueKey.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
                message = "Error; Edit failed.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion
    }
}
