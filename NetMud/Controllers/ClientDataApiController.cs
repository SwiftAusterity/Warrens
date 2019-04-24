using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Player;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    [Authorize]
    public class ClientDataApiController : ApiController
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

        [HttpPost]
        [Route("api/ClientDataApi/ToggleTutorialMode", Name = "ClientDataAPI_ToggleTutorialMode")]
        public JsonResult<bool> ToggleTutorialMode()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            if(user != null)
            {
                user.GameAccount.Config.UITutorialMode = !user.GameAccount.Config.UITutorialMode;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.UITutorialMode);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleSoundMute", Name = "ClientDataAPI_ToggleSoundMute")]
        public JsonResult<bool> ToggleSoundMute()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            if (user != null)
            {
                user.GameAccount.Config.SoundMuted = !user.GameAccount.Config.SoundMuted;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.SoundMuted);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleMusicMute", Name = "ClientDataAPI_ToggleMusicMute")]
        public JsonResult<bool> ToggleMusicMute()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            if (user != null)
            {
                user.GameAccount.Config.MusicMuted = !user.GameAccount.Config.MusicMuted;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.MusicMuted);
        }

        [HttpPost]
        [Route("api/ClientDataApi/ToggleGossipParticipation", Name = "ClientDataAPI_ToggleGossipParticipation")]
        public JsonResult<bool> ToggleGossipParticipation()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            if (user != null)
            {
                user.GameAccount.Config.GossipSubscriber = !user.GameAccount.Config.GossipSubscriber;
                user.GameAccount.Config.Save(user.GameAccount, StaffRank.Admin);
            }

            return Json(user.GameAccount.Config.GossipSubscriber);
        }
		
        [HttpGet]
        [Route("api/ClientDataApi/GetAccountNames", Name = "ClientDataAPI_GetAccountNames")]
        public JsonResult<string[]> GetAccountNames(string term)
        {
            IQueryable<ApplicationUser> accounts = UserManager.Users;

            return Json(accounts.Where(acct => acct.GlobalIdentityHandle.Contains(term)).Select(acct => acct.GlobalIdentityHandle).ToArray());
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetCharacterNamesForAccount/{accountName}", Name = "ClientDataAPI_GetCharacterNamesForAccount")]
        public JsonResult<string[]> GetCharacterNamesForAccount(string accountName, string term)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            IEnumerable<IPlayerTemplate> characters = PlayerDataCache.GetAll().Where(chr => chr.AccountHandle.Equals(user.GlobalIdentityHandle) && chr.Name.Contains(term));

            return Json(characters.Select(chr => chr.Name).ToArray());
        }
    }
}
