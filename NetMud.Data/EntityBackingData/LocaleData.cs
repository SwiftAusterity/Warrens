using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;

namespace NetMud.Data.EntityBackingData
{
    public class LocaleData : EntityBackingDataPartial, ILocaleData
    {
        public override Type EntityClass => throw new NotImplementedException();

        public IZoneData Zone { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HashSet<IRoomData> Rooms { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool AlwaysVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<IZoneData> ZoneExits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IEnumerable<ILocaleData> LocaleExits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public IMap Interior { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public IRoomData CentralRoom(int zIndex = -1)
        {
            throw new NotImplementedException();
        }

        public Tuple<int, int, int> Diameter()
        {
            throw new NotImplementedException();
        }

        public Tuple<int, int, int> FullDimensions()
        {
            throw new NotImplementedException();
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            throw new NotImplementedException();
        }

        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            throw new NotImplementedException();
        }
    }
}
