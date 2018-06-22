using NetMud.Cartography;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.PlayerConfiguration;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Physics;
using System.Web.Http;
using System.Web.Http.Results;

namespace NetMud.Controllers
{
    public class ClientDataApiController : ApiController
    {
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
    }
}
