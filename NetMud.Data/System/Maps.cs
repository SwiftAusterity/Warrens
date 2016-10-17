using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Place;
using System;

namespace NetMud.Data.System
{
    public class Map : IMap
    {
        public long[,,] CoordinatePlane { get; set; }

        public bool Partial { get; private set; }

        public long[,] GetSinglePlane(int zIndex, bool forAdmin = false, bool withPathways = false)
        {
            throw new NotImplementedException();
        }
    }
}
