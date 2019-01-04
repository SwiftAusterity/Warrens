using NetMud.DataStructure.Gaia;
using System;

namespace NetMud.Data.Gaias
{
    /// <summary>
    /// Quality valuation trends
    /// </summary>
    [Serializable]
    public class EconomicTrend : IEconomicTrend
    {
        public string Quality { get; set; }

        public int Basis { get; set; }

        public short Trend { get; set; }

        public decimal Adjustment { get; set; }
    }
}
