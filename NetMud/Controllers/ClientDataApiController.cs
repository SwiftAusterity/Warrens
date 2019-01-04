using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Inanimate;
using NetMud.DataStructure.NPC;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Zone;
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

            System.Collections.Generic.IEnumerable<IPlayerTemplate> characters = PlayerDataCache.GetAll().Where(chr => chr.AccountHandle.Equals(user.GlobalIdentityHandle) && chr.Name.Contains(term));

            return Json(characters.Select(chr => chr.Name).ToArray());
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetEntityInfo/{key}/{typeName}", Name = "ClientDataAPI_GetEntityInfo")]
        public JsonResult<string> GetEntityInfo(string key, string typeName)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            var player = LiveCache.Get<IPlayer>(user.GameAccount.Character.Id);

            if (player == null)
            {
                //error
                return Json("Bad User.");
            }

            string returnString = string.Empty;
            switch (typeName)
            {
                case "Item":
                    var item = LiveCache.Get<IInanimate>(new LiveCacheKey(typeof(IInanimate), key));

                    if (item != null)
                    {
                        returnString = item.RenderToInfo(player);
                    }
                    break;
                case "Player":
                    var playr = LiveCache.Get<IPlayer>(new LiveCacheKey(typeof(IPlayer), key));

                    if (playr != null)
                    {
                        returnString = playr.RenderToInfo(player);
                    }
                    break;
                case "NPC":
                    var npc = LiveCache.Get<INonPlayerCharacter>(new LiveCacheKey(typeof(INonPlayerCharacter), key));

                    if (npc != null)
                    {
                        returnString = npc.RenderToInfo(player);
                    }
                    break;
                case "Zone":
                    var zone = LiveCache.Get<IZone>(new LiveCacheKey(typeof(IZone), key));

                    if (zone != null)
                    {
                        returnString = zone.RenderToInfo(player);
                    }
                    break;
                case "Tile":
                    return Json("Use GetTileInfo.");
            }

            if (string.IsNullOrWhiteSpace(returnString))
            {
                return Json("Invalid target.");
            }

            return Json(returnString);
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetTileInfo/{zoneId}/{x}/{y}", Name = "ClientDataAPI_GetTileInfo")]
        public JsonResult<string> GetTileInfo(long zoneId, int x, int y)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            var player = LiveCache.Get<IPlayer>(user.GameAccount.Character.Id);

            if(player == null)
            {
                //error
                return Json("Bad User.");
            }

            var zone = LiveCache.Get<IZone>(zoneId);

            if(zone == null)
            {
                //error
                return Json("Invalid zone.");
            }
            else if(x > 100 || x < 0 || y > 100 || y < 0)
            {
                //error
                return Json("Invalid coordinates.");
            }

            var tile = zone.Map.CoordinateTilePlane[x, y];

            if(tile == null)
            {
                //error
                return Json("Invalid tile.");
            }

            return Json(tile.RenderToInfo(player));
        }

    }
}
