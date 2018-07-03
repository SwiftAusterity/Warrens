using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Cartography;
using NetMud.Data.System;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.SupportingClasses;
using NetMud.Physics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    public class AdminDataApiController : ApiController
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
        public string[] GetDimensionalData(long id)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(id);

            if (model == null)
                return new string[0];

            return model.ModelPlanes.Select(plane => plane.TagName).Distinct().ToArray();
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderRoomForEditWithRadius/{id}/{radius}", Name = "RenderRoomForEditWithRadius")]
        public string RenderRoomForEditWithRadius(long id, int radius)
        {
            var centerRoom = BackingDataCache.Get<IRoomData>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLocaleMapForEdit/{id}/{zIndex}", Name = "RenderLocaleMapForEdit")]
        public string[] RenderLocaleMapForEdit(long id, int zIndex)
        {
            var locale = BackingDataCache.Get<ILocaleData>(id);

            if (locale == null)
                return new string[] { "Invalid inputs." };

            var maps = Rendering.RenderRadiusMap(locale, 10, zIndex);

            return new string[] { maps.Item1, maps.Item2, maps.Item3 };
        }


        [HttpGet]
        [Route("api/AdminDataApi/GetDictata/{wordType}", Name = "AdminAPI_GetDictata")]
        public JsonResult<string[]> GetDictata(LexicalType wordType, string term)
        {
            var words = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.WordType == wordType && dict.Name.Contains(term));

            return Json(words.Select(word => word.Name).ToArray());
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public string ChangeAccountRole(string accountName, short role)
        {
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            var validRoles = roleManager.Roles.ToList();

            var user = UserManager.FindById(User.Identity.GetUserId());
            var account = user.GameAccount;

            if (string.IsNullOrWhiteSpace(accountName) || account.GlobalIdentityHandle.Equals(accountName) || role >= (short)user.GetStaffRank(User))
                return "failure";

            var userToModify = UserManager.FindByName(accountName);

            if (userToModify == null)
                return "failure";

            var rolesToRemove = userToModify.Roles.Select(rol => validRoles.First(vR => vR.Id.Equals(rol.RoleId)).Name).ToList();

            foreach (var currentRole in rolesToRemove)
                UserManager.RemoveFromRole(userToModify.Id, currentRole);

            UserManager.AddToRole(userToModify.Id, ((StaffRank)role).ToString());

            return "success";
        }
    }
}
