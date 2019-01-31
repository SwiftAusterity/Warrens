using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Commands.Attributes;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Architectural.ActorBase;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.NaturalResource;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Zone;
using NetMud.Models.Features;
using NetMud.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class FeaturesController : Controller
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

        public FeaturesController()
        {
        }

        public FeaturesController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        #region Template Data
        public ActionResult NPCs(string SearchTerm = "")
        {
            try
            {
                IEnumerable<INonPlayerCharacterTemplate> validEntries = TemplateCache.GetAll<INonPlayerCharacterTemplate>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                NPCsViewModel vModel = new NPCsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Items(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IInanimateTemplate> validEntries = TemplateCache.GetAll<IInanimateTemplate>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                ItemsViewModel vModel = new ItemsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Flora(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IFlora> validEntries = TemplateCache.GetAll<IFlora>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                FloraViewModel vModel = new FloraViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Fauna(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IFauna> validEntries = TemplateCache.GetAll<IFauna>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                FaunaViewModel vModel = new FaunaViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Minerals(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IMineral> validEntries = TemplateCache.GetAll<IMineral>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                MineralsViewModel vModel = new MineralsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Races(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IRace> validEntries = TemplateCache.GetAll<IRace>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                RacesViewModel vModel = new RacesViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Worlds(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IGaiaTemplate> validEntries = TemplateCache.GetAll<IGaiaTemplate>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                WorldsViewModel vModel = new WorldsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Zones(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IZoneTemplate> validEntries = TemplateCache.GetAll<IZoneTemplate>(true).Where(zone => zone.AlwaysDiscovered);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                ZonesViewModel vModel = new ZonesViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Locales(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ILocaleTemplate> validEntries = TemplateCache.GetAll<ILocaleTemplate>(true).Where(zone => zone.AlwaysDiscovered);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                LocalesViewModel vModel = new LocalesViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Celestials(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ICelestial> validEntries = TemplateCache.GetAll<ICelestial>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                CelestialsViewModel vModel = new CelestialsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Languages(string SearchTerm = "")
        {
            try
            {

                IEnumerable<ILanguage> validEntries = ConfigDataCache.GetAll<ILanguage>();
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                LanguagesViewModel vModel = new LanguagesViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Materials(string SearchTerm = "")
        {
            try
            {

                IEnumerable<IMaterial> validEntries = TemplateCache.GetAll<IMaterial>(true);
                ApplicationUser user = null;
                string searcher = SearchTerm.Trim().ToLower();

                if (User.Identity.IsAuthenticated)
                {
                    user = UserManager.FindById(User.Identity.GetUserId());
                    StaffRank userRank = user.GetStaffRank(User);
                }

                MaterialsViewModel vModel = new MaterialsViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
                {
                    authedUser = user,
                    SearchTerm = SearchTerm,
                };

                return View(vModel);
            }
            catch (Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }
        #endregion

        public ActionResult Help(string SearchTerm = "", bool IncludeInGame = true)
        {
            List<IHelp> validEntries = TemplateCache.GetAll<IHelp>(true).ToList();
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                StaffRank userRank = user.GetStaffRank(User);
            }

            if (IncludeInGame)
            {
                //All the entities with helps
                IEnumerable<ILookupData> entityHelps = TemplateCache.GetAll<ILookupData>(true).Where(data => !data.ImplementsType<IHelp>());
                validEntries.AddRange(entityHelps.Select(helpful => new Data.Administrative.Help() { Name = helpful.Name, HelpText = helpful.HelpText }));

                //All the commands
                Assembly commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
                IEnumerable<Type> validTargetTypes = commandsAssembly.GetTypes().Where(t => !t.IsAbstract && t.ImplementsType<IHelpful>());

                foreach (Type command in validTargetTypes)
                {
                    IHelpful instance = (IHelpful)Activator.CreateInstance(command);
                    MarkdownString body = instance.HelpText;
                    string subject = command.Name;

                    validEntries.Add(new Data.Administrative.Help() { Name = subject, HelpText = body });
                }
            }

            HelpViewModel vModel = new HelpViewModel(validEntries.Where(help => help.HelpText.ToLower().Contains(searcher) || help.Name.ToLower().Contains(searcher)))
            {
                authedUser = user,
                SearchTerm = SearchTerm,
                IncludeInGame = IncludeInGame
            };

            return View(vModel);
        }

        #region NonDataViews
        public ActionResult Skills()
        {
            return View();
        }

        public ActionResult Lore()
        {
            return View();
        }

        public ActionResult TheWorld()
        {
            return View();
        }
        #endregion
    }
}