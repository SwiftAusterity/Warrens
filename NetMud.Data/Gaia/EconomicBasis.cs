using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using System;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Item type basises
    /// </summary>
    [Serializable]
    public class EconomicBasis : IEconomicBasis
    {
        public IInanimateTemplate ItemType { get; set; }

        public int Basis { get; set; }

        public short Trend { get; set; }

        public decimal Adjustment { get; set; }
    }
}
