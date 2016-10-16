using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using System;

namespace NetMud.Data.System
{
    public class LiveMap : ILiveMap
    {
        public IRoom[,,] CoordinatePlane { get; set; }

        public bool Partial { get; private set; }

        public string RenderToSinglePlane(int zIndex, bool withPathways = false)
        {
            throw new NotImplementedException();
        }
    }

    public class BackingDataMap : IBackingDataMap
    {
        public IRoomData[,,] CoordinatePlane { get; set; }

        public string RenderToSinglePlane(int zIndex, bool forAdmin, bool withPathways)
        {
            throw new NotImplementedException();
        }
    }
}
