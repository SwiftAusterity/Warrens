using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.EntityBackingData;
using NetMud.DataStructure.Base.Supporting;

namespace NetMud.Data.EntityBackingData
{
    public class ZoneData : EntityBackingDataPartial, IZoneData
    {
        public override Type EntityClass => throw new NotImplementedException();

        public HashSet<IAdventureTemplate> Templates { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public HashSet<ILocaleData> Locales { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int BaseElevation { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int TemperatureCoefficient { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public int PressureCoefficient { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            throw new NotImplementedException();
        }
    }
}
