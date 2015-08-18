using NetMud.Data.Reference;
using NetMud.DataAccess;
using System;
using System.Linq;
using System.Web.Http;

namespace NetMud.Controllers
{
    public class ClientDataApiController : ApiController
    {
        public string GetEntityModelView(long modelId, short yaw, short pitch, short roll)
        {
            var model = ReferenceWrapper.GetOne<DimensionalModelData>(modelId);

            if(model == null)
                return String.Empty;

            return model.ViewFlattenedModel(pitch, yaw, roll);
        }

        public string[] GetDimensionalData(long id)
        {
            var model = ReferenceWrapper.GetOne<DimensionalModelData>(id);

            if (model == null)
                return new string[0];

            return model.ModelPlanes.Select(plane => plane.TagName).Distinct().ToArray();
        }
    }
}
