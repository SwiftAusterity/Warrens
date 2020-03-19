using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Linguistic;
using NetMud.Models.Features;
using System;
using System.Collections.Generic;
using System.Linq;
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
                    AuthedUser = user,
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

        public ActionResult Help(string SearchTerm = "")
        {
            List<IHelp> validEntries = TemplateCache.GetAll<IHelp>(true).ToList();
            ApplicationUser user = null;
            string searcher = SearchTerm.Trim().ToLower();

            if (User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                StaffRank userRank = user.GetStaffRank(User);
            }

            HelpViewModel vModel = new HelpViewModel(validEntries.Where(help => help.HelpText.ToLower().Contains(searcher) || help.Name.ToLower().Contains(searcher)))
            {
                AuthedUser = user,
                SearchTerm = SearchTerm
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