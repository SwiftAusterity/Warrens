using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
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

        #region NonDataViews
        public ActionResult TheWorld()
        {
            return View();
        }
        #endregion
    }
}