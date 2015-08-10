using NetMud.Data.Reference;
using NetMud.DataAccess;
using NetMud.Physics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
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
