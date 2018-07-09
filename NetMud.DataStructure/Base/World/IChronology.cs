using NetMud.DataStructure.Base.System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Base.World
{
    /// <summary>
    /// Time keeping config data for a world
    /// </summary>
    public interface IChronology : ILookupData
    {
        /// <summary>
        /// List of monthnames in order
        /// </summary>
        IEnumerable<string> Months { get; set; }

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
