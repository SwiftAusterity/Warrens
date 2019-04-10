using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.AspNet.Identity.Owin;
using NetMud.Authentication;
using NetMud.Data.Players;
using NetMud.Data.Room;
using NetMud.DataAccess;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Administrative;
using NetMud.DataStructure.Room;
using NetMud.Physics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    [Authorize(Roles = "Admin,Builder")]
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
        [Route("api/AdminDataApi/RenderLiveRoomForEditWithRadius/{radius}")]
        public string RenderLiveRoomForEditWithRadius(string birthMark, int radius)
        {
            IRoom centerRoom = LiveCache.Get<IRoom>(new LiveCacheKey(typeof(IRoom), birthMark));

            if (centerRoom == null || radius < 0)
            {
                return "Invalid inputs.";
            }

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        [Route("api/AdminDataApi/RenderLiveLocaleMapForEdit/{zIndex}")]
        public string[] RenderLiveLocaleMapForEdit(string birthMark, int zIndex)
        {
            ILocale locale = LiveCache.Get<ILocale>(new LiveCacheKey(typeof(ILocale), birthMark));

            if (locale == null)
            {
                return new string[] { "Invalid inputs." };
            }

            System.Tuple<string, string, string> maps = Rendering.RenderRadiusMap(locale, 10, zIndex);

            return new string[] { maps.Item1, maps.Item2, maps.Item3 };
        }


        [HttpGet]
        [Route("api/AdminDataApi/GetDictata/{languageCode}/{wordType}/{term}", Name = "AdminAPI_GetDictata")]
        public JsonResult<string[]> GetDictata(string languageCode, LexicalType wordType, string term)
        {
            IEnumerable<IDictata> words = ConfigDataCache.GetAll<IDictata>().Where(dict => dict.WordType == wordType && dict.Name.Contains(term) && dict.Language.GoogleLanguageCode.Equals(languageCode));

            return Json(words.Select(word => word.Name).ToArray());
        }

        [HttpGet]
        [Route("api/AdminDataApi/GetRoomsOfLocale/{localeId}", Name = "AdminAPI_GetRoomsOfLocale")]
        public JsonResult<Dictionary<long, string>> GetRoomsOfLocale(int localeId)
        {
            IEnumerable<IRoomTemplate> rooms = ConfigDataCache.GetAll<IRoomTemplate>().Where(room => room.ParentLocation.Id == localeId);

            return Json(rooms.ToDictionary(room => room.Id, room => room.Name));
        }

        [HttpPost]
        [Route("api/AdminDataApi/ChangeAccountRole/{accountName}/{role}", Name = "AdminAPI_ChangeAccountRole")]
        public string ChangeAccountRole(string accountName, short role)
        {
            RoleManager<IdentityRole> roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(new ApplicationDbContext()));
            List<IdentityRole> validRoles = roleManager.Roles.ToList();

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

            List<string> rolesToRemove = userToModify.Roles.Select(rol => validRoles.First(vR => vR.Id.Equals(rol.RoleId)).Name).ToList();

            foreach (string currentRole in rolesToRemove)
            {
                UserManager.RemoveFromRole(userToModify.Id, currentRole);
            }

            UserManager.AddToRole(userToModify.Id, ((StaffRank)role).ToString());

            return "success";
        }

        [HttpPost]
        [Route("api/AdminDataApi/Quickbuild/{originId}/{destinationId}/{direction}/{incline}", Name = "AdminAPI_Quickbuild")]
        public string Quickbuild(long originId, long destinationId, int direction, int incline)
        {
            ApplicationUser authedUser = UserManager.FindById(User.Identity.GetUserId());

            IRoomTemplate origin = TemplateCache.Get<IRoomTemplate>(originId);
            IRoomTemplate destination = TemplateCache.Get<IRoomTemplate>(destinationId);

            string message = string.Empty;

            if (destination == null)
            {
                destination = new RoomTemplate
                {
                    Name = "Room",
                    Medium = origin.Medium,
                    ParentLocation = origin.ParentLocation,
                    Model = new DimensionalModel()
                    {
                        ModelTemplate = origin.Model.ModelTemplate,
                        Composition = origin.Model.Composition,
                        Height = origin.Model.Height,
                        Length = origin.Model.Length,
                        Width = origin.Model.Width,
                        SurfaceCavitation = origin.Model.SurfaceCavitation,
                        Vacuity = origin.Model.Vacuity
                    }
                };

                destination = (IRoomTemplate)destination.Create(authedUser.GameAccount, authedUser.GetStaffRank(User));
            }


            if (destination != null)
            {
                IPathwayTemplate newObj = new PathwayTemplate
                {
                    Name = "Pathway",
                    Destination = destination,
                    Origin = origin,
                    InclineGrade = incline,
                    DegreesFromNorth = direction,
                    Model = new DimensionalModel()
                    {
                        ModelTemplate = origin.Model.ModelTemplate,
                        Composition = origin.Model.Composition,
                        Height = origin.Model.Height,
                        Length = origin.Model.Length,
                        Width = origin.Model.Width,
                        SurfaceCavitation = origin.Model.SurfaceCavitation,
                        Vacuity = origin.Model.Vacuity
                    }
                };

                if (newObj.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                {
                    message = "Error; Creation failed.";
                }
                else
                {
                    PathwayTemplate reversePath = new PathwayTemplate
                    {
                        Name = newObj.Name,
                        DegreesFromNorth = Utilities.ReverseDirection(newObj.DegreesFromNorth),
                        Origin = newObj.Destination,
                        Destination = newObj.Origin,
                        Model = newObj.Model,
                        InclineGrade = newObj.InclineGrade * -1
                    };

                    if (reversePath.Create(authedUser.GameAccount, authedUser.GetStaffRank(User)) == null)
                    {
                        message = "Reverse Path creation FAILED. Origin path creation SUCCESS.";
                    }

                    LoggingUtility.LogAdminCommandUsage("*WEB* - Quickbuild[" + newObj.Id.ToString() + "]", authedUser.GameAccount.GlobalIdentityHandle);
                }
            }
            else
            {
                message = "Error; Creation failed.";
            }

            return message;
        }
    }
}
