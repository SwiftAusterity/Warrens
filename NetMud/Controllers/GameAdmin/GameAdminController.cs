using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Game;
using NetMud.DataStructure.Gossip;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.System;
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
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                Journals = TemplateCache.GetAll<IJournalEntry>(),
                GameTemplates = TemplateCache.GetAll<IGameTemplate>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LivePlayers = LiveCache.GetAll<IPlayer>().Count(),

                ConfigDataObject = globalConfig,
                AdminsOnly = globalConfig.AdminsOnly,
                UserCreationActive = globalConfig.UserCreationActive,

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
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownLoop(processName, 600);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningProcess[" + processName + "]", authedUser.GameAccount.GlobalIdentityHandle);
            string message = "Cancel signal sent.";

            return RedirectToAction("Index", new { Message = message });
        }

        [Authorize(Roles = "Admin")]
        public ActionResult StopRunningAllProcess()
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            Processor.ShutdownAll(600);

            LoggingUtility.LogAdminCommandUsage("*WEB* - StopRunningALLPROCESSES", authedUser.GameAccount.GlobalIdentityHandle);
            string message = "Cancel signal sent for entire world.";

            return RedirectToAction("Index", new { Message = message });
        }
        #endregion

        #region Running Data
        [Authorize(Roles = "Admin")]
        public ActionResult BackupWorld(string BackupName = "")
        {
            HotBackup hotBack = new HotBackup();

            hotBack.WriteLiveBackup(BackupName);
            Templates.WriteFullBackup(BackupName);
            ConfigData.WriteFullBackup(BackupName);

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
                .Where(player => player.Template<IPlayerTemplate>().Account.Config.GossipSubscriber)
                .Select(player => new Member()
                {
                    Name = player.AccountHandle,
                    WriteTo = (message) => player.WriteTo(new string[] { message })
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
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            globalConfig.AdminsOnly = vModel.AdminsOnly;
            globalConfig.UserCreationActive = vModel.UserCreationActive;

            string message;
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

            string message;
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
