using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Cartography;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Player;
using NetMud.DataStructure.Room;
using NetMud.Physics;
using System;
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
            }

            return Json(user.GameAccount.Config.UITutorialMode);
        }
		
        [HttpGet]
        public string GetEntityModelView(long modelId)
        {
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
                return string.Empty;

            return Render.FlattenModelForWeb(model);
        }

        [HttpGet]
        public string RenderRoomWithRadius(long id, int radius)
        {
            IRoomTemplate centerRoom = TemplateCache.Get<IRoomTemplate>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            return Rendering.RenderRadiusMap(centerRoom, radius, false);
        }

        [HttpGet]
        public JsonResult<IUIModule> GetUIModuleContent(string moduleName)
        {
            IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

            if (module != null)
                return Json(module);

            return null;
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
		
        [HttpPost]
        public string RemoveUIModuleContent(string moduleName, int location)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return "Invalid Account.";
            }

            List<Tuple<IUIModule, int>> modules = account.Config.UIModules.ToList();
            if (moduleName == "**anymodule**" && location != -1)
            {
                if (modules.Any(mod => mod.Item2.Equals(location)))
                {
                    Tuple<IUIModule, int> moduleTuple = modules.FirstOrDefault(mod => mod.Item2.Equals(location));
                    modules.Remove(moduleTuple);
                }
            }
            else
            {
                IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

                if (module == null)
                {
                    return "Invalid Module.";
                }

                if ((location < 1 && location != -1) || location > 4)
                {
                    return "Invalid Location";
                }

                Tuple<IUIModule, int> moduleTuple = new Tuple<IUIModule, int>(module, location);

                //Remove this module
                if (modules.Any(mod => mod.Item1.Equals(module) && mod.Item2.Equals(location)))
                    modules.Remove(moduleTuple);
            }

            account.Config.UIModules = modules;
            account.Config.Save(account, StaffRank.Player);

            return "Success";
        }

        [HttpPost]
        public string SaveUIModuleContent(string moduleName, int location)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return "Invalid Account.";
            }

            IUIModule module = TemplateCache.GetByName<IUIModule>(moduleName);

            if (module == null)
            {
                return "Invalid Module.";
            }

            if ((location < 1 && location != -1) || location > 4)
            {
                return "Invalid Location";
            }

            List<Tuple<IUIModule, int>> modules = account.Config.UIModules.ToList();
            Tuple<IUIModule, int> moduleTuple = new Tuple<IUIModule, int>(module, location);

            //Remove this module
            if (modules.Any(mod => mod.Item1.Equals(module)))
                modules.Remove(moduleTuple);

            //Remove the module in its place
            if (location != -1 && modules.Any(mod => mod.Item2.Equals(location)))
                modules.RemoveAll(mod => mod.Item2.Equals(location));

            //Add it finally
            modules.Add(moduleTuple);

            account.Config.UIModules = modules;

            account.Config.Save(account, StaffRank.Player);

            return "Success";
        }

        [HttpGet]
        public JsonResult<IEnumerable<Tuple<IUIModule, int>>> LoadUIModules()
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return null;
            }

            return Json(account.Config.UIModules);
        }

        [HttpGet]
        [Route("api/ClientDataApi/GetUIModuleNames", Name = "ClientDataAPI_GetUIModuleNames")]
        public JsonResult<string[]> GetUIModuleNames(string term)
        {
            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());

            Data.Players.Account account = user.GameAccount;

            if (account == null)
            {
                return Json(new string[0]);
            }

            IEnumerable<IUIModule> modules = TemplateCache.GetAll<IUIModule>(true).Where(uim => uim.Name.Contains(term));

            return Json(modules.Select(mod => mod.Name).ToArray());
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
    }
}
