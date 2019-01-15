using NetMud.DataStructure.Inanimate;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Item type basises
    /// </summary>
    public interface IEconomicBasis
    {
        /// <summary>
        /// The item template this is for
        /// </summary>
        IInanimateTemplate ItemType { get; set; }

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
