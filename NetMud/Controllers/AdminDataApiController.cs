using NetMud.Cartography;
using NetMud.DataAccess.Cache;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Base.Supporting;
using NetMud.Physics;
using System;
using System.Linq;
using System.Web.Http;

namespace NetMud.Controllers
{
    public class AdminDataApiController : ApiController
    {
        [HttpGet]
        public string GetEntityModelView(long modelId)
        {
            var model = BackingDataCache.Get<IDimensionalModelData>(modelId);

            if (model == null)
                return String.Empty;

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
        public string RenderRoomForEditWithRadius(long id, int radius)
        {
            var centerRoom = BackingDataCache.Get<IRoomData>(id);

            if (centerRoom == null || radius < 0)
                return "Invalid inputs.";

            return Rendering.RenderRadiusMap(centerRoom, radius);
        }

        [HttpGet]
        public string RenderLocaleMapForEdit(long id)
        {
            var locale = BackingDataCache.Get<ILocaleData>(id);

            if (locale == null)
                return "Invalid inputs.";

            return Rendering.RenderRadiusMap(locale, 10);
        }
    }
}
