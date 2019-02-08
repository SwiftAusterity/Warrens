using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Gossip;
using NetMud.Models.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading.Tasks;
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
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));
            IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));

            DashboardViewModel dashboardModel = new DashboardViewModel
            {
                authedUser = UserManager.FindById(User.Identity.GetUserId()),

                Inanimates = TemplateCache.GetAll<IInanimateTemplate>(),
                NPCs = TemplateCache.GetAll<INonPlayerCharacterTemplate>(),
                Zones = TemplateCache.GetAll<IZoneTemplate>(),
                Worlds = TemplateCache.GetAll<IGaiaTemplate>(),
                Locales = TemplateCache.GetAll<ILocaleTemplate>(),
                Rooms = TemplateCache.GetAll<IRoomTemplate>(),

                HelpFiles = TemplateCache.GetAll<IHelp>(),
                Races = TemplateCache.GetAll<IRace>(),
                Celestials = TemplateCache.GetAll<ICelestial>(),
                Journals = TemplateCache.GetAll<IJournalEntry>(),
                DimensionalModels = TemplateCache.GetAll<IDimensionalModelData>(),
                Flora = TemplateCache.GetAll<IFlora>(),
                Fauna = TemplateCache.GetAll<IFauna>(),
                Minerals = TemplateCache.GetAll<IMineral>(),
                Materials = TemplateCache.GetAll<IMaterial>(),
                DictionaryWords = ConfigDataCache.GetAll<IDictata>(),
                Languages = ConfigDataCache.GetAll<ILanguage>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LivePlayers = LiveCache.GetAll<IPlayer>().Count(),
                LiveInanimates = LiveCache.GetAll<IInanimate>().Count(),
                LiveNPCs = LiveCache.GetAll<INonPlayerCharacter>().Count(),
                LiveZones = LiveCache.GetAll<IZone>().Count(),
                LiveWorlds = LiveCache.GetAll<IGaia>().Count(),
                LiveLocales = LiveCache.GetAll<ILocale>().Count(),
                LiveRooms = LiveCache.GetAll<IRoom>().Count(),

                ConfigDataObject = globalConfig,
                WebsocketPortalActive = globalConfig.WebsocketPortalActive,
                AdminsOnly = globalConfig.AdminsOnly,
                UserCreationActive = globalConfig.UserCreationActive,
                BaseLanguage = globalConfig.BaseLanguage,
                AzureTranslationKey = globalConfig.AzureTranslationKey,
                TranslationActive = globalConfig.TranslationActive,

                QualityChange = new string[0],
                QualityChangeValue = new int[0],

                ValidZones = TemplateCache.GetAll<IZoneTemplate>(true),
                ValidLanguages = ConfigDataCache.GetAll<ILanguage>(),

                GossipConfigDataObject = gossipConfig,
                GossipActive = gossipConfig.GossipActive,
                ClientId = gossipConfig.ClientId,
                ClientSecret = gossipConfig.ClientSecret,
                ClientName = gossipConfig.ClientName,
                SuspendMultiplier = gossipConfig.SuspendMultiplier,
                SuspendMultiplierMaximum = gossipConfig.SuspendMultiplierMaximum,
                SupportedChannels = gossipConfig.SupportedChannels,
                SupportedFeatures = gossipConfig.SupportedFeatures
            };

            return View(dashboardModel);
        }

        public ActionResult ModalErrorOrClose(string Message = "")
        {
            return View("~/Views/GameAdmin/ModalErrorOrClose.cshtml", "_chromelessLayout", Message);
        }

        #region Live Threads
        [Authorize(Roles = "Admin")]
        public ActionResult StopRunningProcess(string processName)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownLoop(processName, 600, "{0} seconds before " + processName + " is shutdown.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningProcess[" + processName + "]", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult StopRunningAllProcess()
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownAll(600, "{0} seconds before TOTAL WORLD SHUTDOWN.", 60);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        [Authorize(Roles = "Admin")]
        public ActionResult BackupWorld()
        {
            HotBackup hotBack = new HotBackup();

            hotBack.WriteLiveBackup();
            Templates.WriteFullBackup();

            return RedirectToAction("Index", new { Message = "Backup Started" });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RestoreWorld()
        {
            HotBackup hotBack = new HotBackup();

            //TODO: Ensure we suspend EVERYTHING going on (fights, etc), add some sort of announcement globally and delay the entire thing on a timer

            //Write the players out first to maintain their positions
            hotBack.WritePlayers();

            //restore everything
            hotBack.RestoreLiveBackup();

            return RedirectToAction("Index", new { Message = "Restore Started" });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult RestartGossipServer()
        {
            IEnumerable<WebSocket> gossipServers = LiveCache.GetAll<WebSocket>();

            foreach (WebSocket server in gossipServers)
            {
                server.Abort();
            }

            IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));
            Func<Member[]> playerList = () => LiveCache.GetAll<IPlayer>()
                .Where(player => player.Descriptor != null && player.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
                .Select(player => new Member()
                {
                    Name = player.AccountHandle,
                    WriteTo = (message) => player.WriteTo(new string[] { message }),
                    BlockedMembers = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => !acq.IsFriend).Select(acq => acq.PersonHandle),
                    Friends = player.Template<IPlayerTemplate>().Account.Config.Acquaintences.Where(acq => acq.IsFriend).Select(acq => acq.PersonHandle)
                }).ToArray();

            void exceptionLogger(Exception ex) => LoggingUtility.LogError(ex);
            void activityLogger(string message) => LoggingUtility.Log(message, LogChannels.GossipServer);

            GossipClient gossipServer = new GossipClient(gossipConfig, exceptionLogger, activityLogger, playerList);

            Task.Run(() => gossipServer.Launch());

            LiveCache.Add(gossipServer, "GossipWebClient");

            return RedirectToAction("Index", new { Message = "Gossip Server Restarted" });
        }
        #endregion

        #region "Global Config"
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GlobalConfig(DashboardViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            globalConfig.WebsocketPortalActive = vModel.WebsocketPortalActive;
            globalConfig.AdminsOnly = vModel.AdminsOnly;
            globalConfig.UserCreationActive = vModel.UserCreationActive;
            globalConfig.BaseLanguage = vModel.BaseLanguage;
            globalConfig.AzureTranslationKey = vModel.AzureTranslationKey;
            globalConfig.TranslationActive = vModel.TranslationActive;

            if (globalConfig.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditGlobalConfig[" + globalConfig.UniqueKey.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public ActionResult GossipConfig(DashboardViewModel vModel)
        {
            string message = string.Empty;
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            IGossipConfig gossipConfig = ConfigDataCache.Get<IGossipConfig>(new ConfigDataCacheKey(typeof(IGossipConfig), "GossipSettings", ConfigDataType.GameWorld));

            gossipConfig.GossipActive = vModel.GossipActive;
            gossipConfig.ClientId = vModel.ClientId;
            gossipConfig.ClientName = vModel.ClientName;
            gossipConfig.ClientSecret = vModel.ClientSecret;
            gossipConfig.SuspendMultiplierMaximum = vModel.SuspendMultiplierMaximum;
            gossipConfig.SuspendMultiplier = vModel.SuspendMultiplier;
            gossipConfig.SupportedChannels = vModel.SupportedChannels;
            gossipConfig.SupportedFeatures = vModel.SupportedFeatures;

            if (gossipConfig.Save(authedUser.GameAccount, authedUser.GetStaffRank(User)))
            {
                LoggingUtility.LogAdminCommandUsage("*WEB* - EditGossipConfig[" + gossipConfig.UniqueKey.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                message = "Edit Successful.";
            }
            else
            {
                message = "Error; Edit failed.";
            }

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion
    }
}
