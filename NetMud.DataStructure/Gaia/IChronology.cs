using System.Collections.Generic;

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
        HashSet<string> Months { get; set; }

        /// <summary>
        /// How many days are per month
        /// </summary>
        int DaysPerMonth { get; set; }

        /// <summary>
        /// How many hours per day
        /// </summary>
        int HoursPerDay { get; set; }

        /// <summary>
        /// The year this starts at when reset from nothing
        /// </summary>
        int StartingYear { get; set; }
    }
}
