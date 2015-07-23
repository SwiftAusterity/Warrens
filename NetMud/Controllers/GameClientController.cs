using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Models;
using NetMud.Authentication;

namespace Controllers
{
    public class GameClientController : Controller
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

        public GameClientController()
        {
        }

        public GameClientController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        public ActionResult Index()
        {
            var model = new GameContextModel();
            model.authedUser = UserManager.FindById(User.Identity.GetUserId());

            return View(model);
        }
    }
}