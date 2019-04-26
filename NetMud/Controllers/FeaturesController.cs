using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Commands.Attributes;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Combat;
using NetMud.DataStructure.System;
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
                AuthedUser = user,
                SearchTerm = SearchTerm,
                IncludeInGame = IncludeInGame
            };

            return View(vModel);
        }

        public ActionResult FightingArts(string SearchTerm = "")
        {
            List<IFightingArt> validEntries = TemplateCache.GetAll<IFightingArt>(true).ToList();
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                StaffRank userRank = user.GetStaffRank(User);
            }

            FightingArtsViewModel vModel = new FightingArtsViewModel(validEntries.Where(help => help.Name.ToLower().Contains(searcher)))
            {
                AuthedUser = user,
                SearchTerm = SearchTerm
            };

            return View(vModel);
        }

        #region NonDataViews
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