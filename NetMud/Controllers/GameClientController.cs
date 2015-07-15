using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetMud.Models;
using System.Web.Mvc;

namespace Controllers
{
    public class GameClientController : Controller
    {
        /// <summary>
        /// Application DB context
        /// </summary>
        protected ApplicationDbContext ApplicationDbContext { get; set; }

        /// <summary>
        /// User manager - attached to application DB context
        /// </summary>
        protected UserManager<ApplicationUser> UserManager { get; set; }

        // GET: GameClient
        public ActionResult Index()
        {
            var model = new GameContextModel();
            model.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View(model);
        }

        private void GetCurrentUserContext()
        {
            this.ApplicationDbContext = new ApplicationDbContext();
            this.UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(this.ApplicationDbContext));
        }
    }
}