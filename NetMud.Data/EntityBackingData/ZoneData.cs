using System;
using System.Collections.Generic;
using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.Place;
using NetMud.DataStructure.Behaviors.System;

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
        public Tuple<IZoneData, bool> ZoneExits { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Dictionary<INaturalResource, int> NaturalResourceSpawn { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public Biome BaseBiome { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override Tuple<int, int, int> GetModelDimensions()
        {
            throw new NotImplementedException();
        }
    }
}
