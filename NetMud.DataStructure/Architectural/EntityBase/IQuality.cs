using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Architectural.EntityBase
{
    /// <summary>
    /// Tracked statistics and quest progress
    /// </summary>
    public interface IQuality
    {
        [Display(Name = "Quality", Description = "The name of the quality.")]
        [DataType(DataType.Text)]
        string Name { get; set; }

        [Display(Name = "Value", Description = "The value/rating of the quality.")]
        [DataType(DataType.Text)]
        int Value { get; set; }

        [Display(Name = "Visible", Description = "Is this quality visible to players?")]
        [DataType(DataType.Text)]
        bool Visible { get; set; }

        [Display(Name = "Type", Description = "The supertype of the quality, almost always use Aspect.")]
        [DataType(DataType.Text)]
        QualityType Type { get; set; }
    }
}
