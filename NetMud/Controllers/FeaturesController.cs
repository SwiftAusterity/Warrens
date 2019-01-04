using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Commands.Attributes;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.System;
using NetMud.DataStructure.Tile;
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
            catch(Exception ex)
            {
                LoggingUtility.LogError(ex);
            }

            return View();
        }

        public ActionResult Items(string SearchTerm = "")
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

        public ActionResult TileTypes(string SearchTerm = "")
        {
            IEnumerable<ITileTemplate> validEntries = TemplateCache.GetAll<ITileTemplate>(true);
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                StaffRank userRank = user.GetStaffRank(User);
            }

            TileTypeViewModel vModel = new TileTypeViewModel(validEntries.Where(item => item.Name.ToLower().Contains(searcher)))
            {
                authedUser = user,
                SearchTerm = SearchTerm,
            };

            return View(vModel);
        }

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

            if(IncludeInGame)
            {
                //All the entities with helps
                var entityHelps = TemplateCache.GetAll<ILookupData>(true).Where(data => !data.ImplementsType<IHelp>());
                validEntries.AddRange(entityHelps.Select(helpful => new Data.Administrative.Help() { Name = helpful.Name, HelpText = helpful.HelpText }));

                //All the commands
                Assembly commandsAssembly = Assembly.GetAssembly(typeof(CommandParameterAttribute));
                var validTargetTypes = commandsAssembly.GetTypes().Where(t => !t.IsAbstract && t.ImplementsType<IHelpful>());

                foreach(var command in validTargetTypes)
                {
                    var instance = (IHelpful)Activator.CreateInstance(command);
                    var body = instance.HelpText;
                    var subject = command.Name;

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
        public ActionResult Interactions()
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