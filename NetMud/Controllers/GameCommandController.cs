using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using NetMud.Models;
using NetMud;
using NetMud.Interp;
using System.Web.Http;

namespace Controllers
{
    public class GameCommandController : ApiController
    {
        private ApplicationUserManager _userManager;
        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.Current.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        public GameCommandController()
        {
        }

        public GameCommandController(ApplicationUserManager userManager)
        {
            UserManager = userManager;
        }

        [HttpGet]
        [AllowAnonymous]//for testing
        public string RenderCommand(string command)
        {
            var authedUser = UserManager.FindById(User.Identity.GetUserId());

            var currentCharacter = authedUser.GameAccount.Characters.FirstOrDefault(ch => ch.ID.Equals(authedUser.GameAccount.CurrentlySelectedCharacter));

            if(currentCharacter == null)
                return "<p>No character selected</p>";

            return Interpret.Render(command, currentCharacter);
        }
    }
}