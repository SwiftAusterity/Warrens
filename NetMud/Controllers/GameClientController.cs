using NetMud.Authentication;
using NetMud.DataAccess;
using NetMud.Models;
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{
    [Authorize]
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
            GameContextModel model = new()
            {
                AuthedUser = UserManager.FindById(User.Identity.GetUserId())
            };

            model.MusicTracks = ContentUtility.GetMusicTracksForZone(model.AuthedUser.GameAccount.GetCurrentlySelectedCharacter()?.CurrentLocation?.CurrentZone);
            model.MusicPlaylists = model.AuthedUser.GameAccount.Config.Playlists;
            return View(model);
        }

        [HttpGet]
        public ActionResult ModularWindow()
        {
            return View("~/Views/GameClient/ModularWindow.cshtml");
        }
    }
}