namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Quality valuation trends
    /// </summary>
    public interface IEconomicTrend
    {
        string Quality { get; set; }

        int Basis { get; set; }

        short Trend { get; set; }

        decimal Adjustment { get; set; }
    }
}
