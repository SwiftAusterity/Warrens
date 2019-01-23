using NetMud.Data.Architectural.DataIntegrity;
using NetMud.DataStructure.Gaia;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NetMud.Data.Gaia
{
    /// <summary>
    /// Time keeping config data for a world
    /// </summary>
    [Serializable]
    public class Chronology : IChronology
    {
        /// <summary>
        /// List of monthnames in order
        /// </summary>
        [Display(Name = "Month Name", Description = "A name of the month, in order.")]
        [UIHint("TagContainer")]
        [Required]
        public HashSet<string> Months { get; set; }

        /// <summary>
        /// How many days are per month
        /// </summary>
        [IntDataIntegrity("Days Per Month must be greater than 0.", 1)]
        [Display(Name = "Days per Month", Description = "How many days there are per month.")]
        [DataType(DataType.Text)]
        [Required]
        public int DaysPerMonth { get; set; }

        /// <summary>
        /// How many hours per day
        /// </summary>
        [IntDataIntegrity("Hours Per Day must be greater than 0.", 1)]
        [Display(Name = "Hours per Day", Description = "How many hours there are per day.")]
        [DataType(DataType.Text)]
        [Required]
        public int HoursPerDay { get; set; }

        /// <summary>
        /// The year this starts at when reset from nothing
        /// </summary>
        [Display(Name = "Reset Year", Description = "What year this world resets to if made from nothing.")]
        [DataType(DataType.Text)]
        [Required]
        public int StartingYear { get; set; }

        public Chronology()
        {
            Months = new HashSet<string>();
        }
    }
}
