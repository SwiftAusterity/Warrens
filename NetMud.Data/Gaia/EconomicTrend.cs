using NetMud.DataStructure.Gaia;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Quality valuation trends
    /// </summary>
    [Serializable]
    public class EconomicTrend : IEconomicTrend
    {
        /// <summary>
        /// The quality this trend affects
        /// </summary>
        [Display(Name = "Quality", Description = "The Quality these values affect.")]
        [DataType(DataType.Text)]
        public string Quality { get; set; }

        /// <summary>
        /// The base value for the quality
        /// </summary>
        [Range(0, 10000, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Basis", Description = "The base value for the quality.")]
        [DataType(DataType.Text)]
        public int Basis { get; set; }

        /// <summary>
        /// Where is the trending currently moving? (positive or negative percentage)
        /// </summary>
        [Range(-100, 100, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Trend", Description = "Where is the trending currently moving? (positive or negative percentage).")]
        [DataType(DataType.Text)]
        public short Trend { get; set; }

        /// <summary>
        /// The current inflationary adjustment
        /// </summary>
        [Range(-1, 1, ErrorMessage = "The {0} must be between {2} and {1}.")]
        [Display(Name = "Adjustment", Description = "he current inflationary adjustment.")]
        [DataType(DataType.Text)]
        public decimal Adjustment { get; set; }
    }
}
