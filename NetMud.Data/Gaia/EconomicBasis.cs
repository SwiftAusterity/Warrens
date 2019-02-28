using NetMud.Data.Architectural.PropertyBinding;
using NetMud.DataStructure.Gaia;
using NetMud.DataStructure.Inanimate;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Item type basises
    /// </summary>
    [Serializable]
    public class EconomicBasis : IEconomicBasis
    {
        /// <summary>
        /// The item template this is for
        /// </summary>
        [UIHint("InanimateTemplateList")]
        [InanimateTemplateDataBinder]
        public IInanimateTemplate ItemType { get; set; }

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
