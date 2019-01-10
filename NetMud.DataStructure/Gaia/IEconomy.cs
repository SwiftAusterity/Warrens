using NetMud.DataStructure.Inanimate;
using System.Collections.Generic;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Economy parent object for keeping track of economic trends
    /// </summary>
    public interface IEconomy
    {
        /// <summary>
        /// Item type basises
        /// </summary>
        HashSet<IEconomicBasis> Bases { get; set; }

        /// <summary>
        /// Quality valuation trends
        /// </summary>
        HashSet<IEconomicTrend> Trends { get; set; }

        /// <summary>
        /// Forces the system to valuate the object type and adds it to the Bases
        /// </summary>
        /// <param name="basis">the item to value</param>
        /// <returns>the new value</returns>
        decimal MakeValuation(IInanimateTemplate basis);

        /// <summary>
        /// Forces the system to valuate the quality and adds it to the Trends
        /// </summary>
        /// <param name="trend">the quality to value</param>
        /// <returns>the new value</returns>
        decimal MakeValuation(string trend);

        /// <summary>
        /// Adjust the bases
        /// </summary>
        void Adjust(IInanimateTemplate basis, decimal movement);

        /// <summary>
        /// Adjust the trends
        /// </summary>
        void Adjust(string trend, decimal movement);
    }
}
