using NetMud.Data.DataIntegrity;
using NetMud.DataStructure.Base.World;
using System;
using System.Collections.Generic;

namespace NetMud.Data.System
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
        public IEnumerable<string> Months { get; set; }

        /// <summary>
        /// How many days are per month
        /// </summary>
        [IntDataIntegrity("Days Per Month must be greater than 0.", 0)]
        public int DaysPerMonth { get; set; }

        /// <summary>
        /// How many hours per day
        /// </summary>
        [IntDataIntegrity("Hours Per Day must be greater than 0.", 0)]
        public int HoursPerDay { get; set; }

        /// <summary>
        /// The year this starts at when reset from nothing
        /// </summary>
        public int StartingYear { get; set; }
    }
}
