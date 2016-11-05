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
        public string RenderWorldMap(long id, int zIndex)
        {
            var world = BackingDataCache.Get<IWorld>(id);

            if (world == null || zIndex < 0 || zIndex > world.WorldMap.CoordinatePlane.GetUpperBound(2))
                return "Invalid inputs.";

            return Rendering.RenderMap(Cartographer.GetSinglePlane(world.WorldMap.CoordinatePlane, zIndex), true, true, null);
        }

        [HttpGet]
        public string RenderZoneMap(long id, int zIndex)
        {
            var zone = BackingDataCache.Get<IZone>(id);

            if (zone == null || zIndex < 0 || zIndex > zone.ZoneMap.CoordinatePlane.GetUpperBound(2))
                return "Invalid inputs.";

            return Rendering.RenderMap(Cartographer.GetSinglePlane(zone.ZoneMap.CoordinatePlane, zIndex), true, true, zone.CentralRoom(zIndex));
        }
    }
}
