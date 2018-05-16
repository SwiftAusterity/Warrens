using NetMud.DataStructure.Base.Place;
using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.Data.Game;

namespace NetMud.Data.EntityBackingData
{
    public class LocaleData : EntityBackingDataPartial, ILocaleData
    {
        public override Type EntityClass
        {
            get { return typeof(Locale); }
        }

        public IZoneData Zone { get; set; }
        public HashSet<IRoomData> Rooms { get; set; }
        public bool AlwaysVisible { get; set; }
        public IEnumerable<IZoneData> ZoneExits { get; set; }
        public IEnumerable<ILocaleData> LocaleExits { get; set; }
        public IMap Interior { get; set; }

        public IRoomData CentralRoom(int zIndex = -1)
        {
            return null;
        }

        public Tuple<int, int, int> Diameter()
        {
            return new Tuple<int, int, int>(0, 0, 0);
        }

        public Tuple<int, int, int> FullDimensions()
        {
            return new Tuple<int, int, int>(0, 0, 0);
        }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            return new Tuple<int, int, int>(0, 0, 0);
        }

        public string RenderMap(int zIndex, bool forAdmin = false)
        {
            return string.Empty;
        }
    }
}
