using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// Time keeping config data for a world
    /// </summary>
    public interface IChronology
    {
        /// <summary>
        /// List of monthnames in order
        /// </summary>
        [Display(Name = "Month Name", Description = "A name of the month, in order.")]
        [UIHint("TagContainer")]
        [Required]
        HashSet<string> Months { get; set; }

        /// <summary>
        /// How many days are per month
        /// </summary>
        [Display(Name = "Days per Month", Description = "How many days there are per month.")]
        [DataType(DataType.Text)]
        [Required]
        int DaysPerMonth { get; set; }

        /// <summary>
        /// How many hours per day
        /// </summary>
        [Display(Name = "Hours per Day", Description = "How many hours there are per day.")]
        [DataType(DataType.Text)]
        [Required]
        int HoursPerDay { get; set; }

        /// <summary>
        /// The year this starts at when reset from nothing
        /// </summary>
        [Display(Name = "Reset Year", Description = "What year this world resets to if made from nothing.")]
        [DataType(DataType.Text)]
        [Required]
        int StartingYear { get; set; }
    }
}
