using NetMud.Data.Reference;
using NetMud.DataAccess;
using System;
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
    }
}
