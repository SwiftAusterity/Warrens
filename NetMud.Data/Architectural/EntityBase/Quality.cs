using NetMud.DataStructure.Architectural.EntityBase;
using System;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Architectural.EntityBase
{
    /// <summary>
    /// Tracked statistics and quest progress
    /// </summary>
    [Serializable]
    public class Quality : IQuality
    {
        [Display(Name = "Quality", Description = "The name of the quality.")]
        [DataType(DataType.Text)]
        public string Name { get; set; }

        [Display(Name = "Value", Description = "The value/rating of the quality.")]
        [DataType(DataType.Text)]
        public int Value { get; set; }

        [Display(Name = "Visible", Description = "Is this quality visible to players?")]
        [DataType(DataType.Text)]
        public bool Visible { get; set; }

        [Display(Name = "Type", Description = "The supertype of the quality, almost always use Aspect.")]
        [DataType(DataType.Text)]
        public QualityType Type { get; set; }
    }
}
