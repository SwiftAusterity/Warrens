using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.Models;
using NetMud.Utility;
using System;
using System.Globalization;
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

        public ActionResult Index(string[] includedTags, string monthYearPair = "")
        {
            System.Collections.Generic.IEnumerable<IJournalEntry> validEntries = Enumerable.Empty<IJournalEntry>();
            System.Collections.Generic.IEnumerable<IJournalEntry> filteredEntries = Enumerable.Empty<IJournalEntry>();
            ApplicationUser user = null;

            if(User.Identity.IsAuthenticated)
            {
                user = UserManager.FindById(User.Identity.GetUserId());
                StaffRank userRank = user.GetStaffRank(User);
                validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && (blog.Public || blog.MinimumReadLevel <= userRank));
            }
            else
            {
                validEntries = TemplateCache.GetAll<IJournalEntry>().Where(blog => blog.IsPublished() && blog.Public);
            }

            System.Collections.Generic.IEnumerable<string> allTags = validEntries.SelectMany(blog => blog.Tags).Distinct();
            if (includedTags != null && includedTags.Count() > 0)
            {
                validEntries = validEntries.Where(blog => blog.Tags.Any(tag => includedTags.Contains(tag)));
            }

            if (!string.IsNullOrWhiteSpace(monthYearPair))
            {
                string[] pair = monthYearPair.Split("|||", StringSplitOptions.RemoveEmptyEntries);
                string month = pair[0];
                int year = -1;

                if (!string.IsNullOrWhiteSpace(month) && int.TryParse(pair[1], out year))
                {
                    filteredEntries = validEntries.Where(blog =>
                        month.Equals(blog.PublishDate.ToString("MMMM", CultureInfo.InvariantCulture), StringComparison.InvariantCultureIgnoreCase)
                        && blog.PublishDate.Year.Equals(year));
                }
            }

            if(filteredEntries.Count() == 0)
            {
                filteredEntries = validEntries;
            }

            BlogViewModel vModel = new BlogViewModel(filteredEntries.OrderByDescending(obj => obj.PublishDate))
             {
                AuthedUser = user,
                MonthYearPairs = validEntries.Select(blog => new Tuple<string, int>(blog.PublishDate.ToString("MMMM", CultureInfo.InvariantCulture), blog.PublishDate.Year)).Distinct(),
                IncludeTags = includedTags?.Where(tag => tag != "false").ToArray() ?? (new string[0]),
                AllTags = allTags.ToArray()
            };

            return View(vModel);
        }
    }
}