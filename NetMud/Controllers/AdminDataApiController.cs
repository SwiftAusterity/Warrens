using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Cartography;
using NetMud.Data.Players;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Architectural.EntityBase;
using NetMud.DataStructure.Linguistic;
using NetMud.DataStructure.Locale;
using NetMud.DataStructure.Room;
using NetMud.Physics;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    [Authorize(Roles = "Admin")]
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
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
            {
                return string.Empty;
            }

            return Render.FlattenModelForWeb(model);
        }

        [HttpGet]
        public string[] GetDimensionalData(long id)
        {
            IDimensionalModelData model = TemplateCache.Get<IDimensionalModelData>(id);

            if (model == null)
            {
                return new string[0];
            }

            return model.ModelPlanes.Select(plane => plane.TagName).Distinct().ToArray();
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderRoomForEditWithRadius/{id}/{radius}", Name = "RenderRoomForEditWithRadius")]
        public string RenderRoomForEditWithRadius(long id, int radius)
        {
            IRoomTemplate centerRoom = TemplateCache.Get<IRoomTemplate>(id);

            if (centerRoom == null || radius < 0)
            {
                return "Invalid inputs.";
            }

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLocaleMapForEdit/{id}/{zIndex}", Name = "RenderLocaleMapForEdit")]
        public string[] RenderLocaleMapForEdit(long id, int zIndex)
        {
            ILocaleTemplate locale = TemplateCache.Get<ILocaleTemplate>(id);

            if (locale == null)
            {
                return new string[] { "Invalid inputs." };
            }

            System.Tuple<string, string, string> maps = Rendering.RenderRadiusMap(locale, 10, zIndex);

            return new string[] { maps.Item1, maps.Item2, maps.Item3 };
        }


        [HttpGet]
        [Route("api/AdminDataApi/GetDictata/{wordType}", Name = "AdminAPI_GetDictata")]
        public JsonResult<string[]> GetDictata(LexicalType wordType, string term)
        {
            System.Collections.Generic.IEnumerable<IDictata> words = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.WordType == wordType && dict.Name.Contains(term));

            return Json(words.Select(word => word.Name).ToArray());
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public string ChangeAccountRole(string accountName, short role)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            System.Collections.Generic.List<IdentityRole> validRoles = roleManager.Roles.ToList();

            ApplicationUser user = UserManager.FindById(User.Identity.GetUserId());
            Account account = user.GameAccount;
            StaffRank userRole = user.GetStaffRank(User);

            if (userRole != StaffRank.Admin)
            {
                if (string.IsNullOrWhiteSpace(accountName) || account.GlobalIdentityHandle.Equals(accountName) || role >= (short)user.GetStaffRank(User))
                {
                    return "failure";
                }
            }

            ApplicationUser userToModify = UserManager.FindByName(accountName);

            if (userToModify == null)
            {
                return "failure";
            }

            System.Collections.Generic.List<string> rolesToRemove = userToModify.Roles.Select(rol => validRoles.First(vR => vR.Id.Equals(rol.RoleId)).Name).ToList();

            foreach (string currentRole in rolesToRemove)
            {
                UserManager.RemoveFromRole(userToModify.Id, currentRole);
            }

            UserManager.AddToRole(userToModify.Id, ((StaffRank)role).ToString());

            return "success";
        }
    }
}
