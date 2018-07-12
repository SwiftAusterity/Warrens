using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace NetMud.Controllers
{
    public class BlogController : Controller
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

        public BlogController()
        {
        }

        public BlogController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index(string SearchTerms = "", int CurrentPageNumber = 1, int ItemsPerPage = 20)
        {
            var validEntries = Enumerable.Empty<IJournalEntry>();
            ApplicationUser user = null;

            if(User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                var userRank = user.GetStaffRank(User);
                validEntries = BackingDataCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
            }
            else
            {
                validEntries = BackingDataCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
            }

            var vModel = new BlogViewModel(validEntries)
             {
                authedUser = user,
                CurrentPageNumber = CurrentPageNumber,
                ItemsPerPage = ItemsPerPage,
                SearchTerms = SearchTerms
            };

            return View(vModel);
        }
    }
}