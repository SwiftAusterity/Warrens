using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Cartography;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Physics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
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

        [HttpGet]
        public string GetEntityModelView(long modelId)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
                return string.Empty;

            return Render.FlattenModelForWeb(model);
        }

        [HttpGet]
        public string RenderRoomWithRadius(long id, int radius)
        {
            var centerRoom = BackingDataCache.Get<IRoomData>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            return Rendering.RenderRadiusMap(centerRoom, radius, false);
        }

        [HttpGet]
        public JsonResult<IUIModule> GetUIModuleContent(string moduleName)
        {
            var module = BackingDataCache.GetByName<IUIModule>(moduleName);

            if (module != null)
                return Json(module);

            return null;
        }

        [HttpPost]
        public string SaveUIModuleContent(string moduleName, int location)
        {
            var user = UserManager.FindById(User.Identity.GetUserId());

            var account = user.GameAccount;

            if (account == null)
            {
                return "Invalid Account.";
            }

            var module = BackingDataCache.GetByName<IUIModule>(moduleName);

            if (module == null)
            {
                return "Invalid Module.";
            }

            if ((location < 1 && location != -1) || location > 4)
            {
                return "Invalid Location";
            }

            var modules = account.Config.UIModules;

            //Remove this module
            if (modules.ContainsKey(module))
                modules.Remove(module);

            //Remove the module in its place
            if (location != -1 && modules.Values.Contains(location))
                modules.Remove(account.Config.UIModules.First(kvp => kvp.Value.Equals(location)));

            //Add it finally
            modules.Add(module, location);

            account.Config.UIModules = modules;

            account.Config.Save(account, StaffRank.Player);

            return "Success";
        }

    }
}
