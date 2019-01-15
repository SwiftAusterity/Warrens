namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Quality valuation trends
    /// </summary>
    public interface IEconomicTrend
    {
        /// <summary>
        /// The quality this trend affects
        /// </summary>
        string Quality { get; set; }

        /// <summary>
        /// The base value for the quality
        /// </summary>
        int Basis { get; set; }

        /// <summary>
        /// Where is the trending currently moving? (positive or negative percentage)
        /// </summary>
        short Trend { get; set; }

        /// <summary>
        /// The current inflationary adjustment
        /// </summary>
        decimal Adjustment { get; set; }
    }
}
