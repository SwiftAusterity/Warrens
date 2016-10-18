using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.SupportingClasses;
using System;

namespace NetMud.Data.System
{
    public class Map : IMap
    {
        public long[,,] CoordinatePlane { get; set; }

        public bool Partial { get; private set; }

        public long[,] GetSinglePlane(int zIndex)
        {
            throw new NotImplementedException();
        }
    }
}
