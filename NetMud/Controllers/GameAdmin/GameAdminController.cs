using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Backup;
using NetMud.CentralControl;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.System;
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
            IGlobalConfig globalConfig = ConfigDataCache.Get<IGlobalConfig>(new ConfigDataCacheKey(typeof(IGlobalConfig), "LiveSettings", ConfigDataType.GameWorld));

            DashboardViewModel dashboardModel = new DashboardViewModel
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId()),

                HelpFiles = TemplateCache.GetAll<IHelp>(),
                Journals = TemplateCache.GetAll<IJournalEntry>(),
                DictionaryWords = ConfigDataCache.GetAll<ILexeme>(),
                Languages = ConfigDataCache.GetAll<ILanguage>(),

                LiveTaskTokens = Processor.GetAllLiveTaskStatusTokens(),
                LiveTaskSubscribers = Processor.GetAllSubscriberStatus(),

                ConfigDataObject = globalConfig,
                AdminsOnly = globalConfig.AdminsOnly,
                UserCreationActive = globalConfig.UserCreationActive,
                BaseLanguage = globalConfig.BaseLanguage,
                AzureTranslationKey = globalConfig.AzureTranslationKey,
                TranslationActive = globalConfig.TranslationActive,
                DeepLexActive = globalConfig.DeepLexActive,
                MirriamDictionaryKey = globalConfig.MirriamDictionaryKey,
                MirriamThesaurusKey = globalConfig.MirriamThesaurusKey,

                ValidLanguages = ConfigDataCache.GetAll<ILanguage>()
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
            Templates.WriteFullBackup(BackupName);
            ConfigData.WriteFullBackup(BackupName);

            return RedirectToAction("Index", new { Message = "Backup Started" });
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
            globalConfig.BaseLanguage = vModel.BaseLanguage;

            globalConfig.AzureTranslationKey = vModel.AzureTranslationKey;
            globalConfig.TranslationActive = vModel.TranslationActive;

            globalConfig.DeepLexActive = vModel.DeepLexActive;
            globalConfig.MirriamDictionaryKey = vModel.MirriamDictionaryKey;
            globalConfig.MirriamThesaurusKey = vModel.MirriamThesaurusKey;

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
        #endregion
    }
}
