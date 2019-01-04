using NetMud.DataStructure.Gaia;
using System;
using System.Linq;

namespace NetMud.Data.Gaias
{
    /// <summary>
    /// An instance of time for the mud
    /// </summary>
    [Serializable]
    public class TimeOfDay : ITimeOfDay
    {
        IChronology BaseChronology { get; set; }

        /// <summary>
        /// Current month
        /// </summary>
        public int Month { get; set; }

        /// <summary>
        /// Current year
        /// </summary>
        public int Year { get; set; }

        /// <summary>
        /// Current day
        /// </summary>
        public int Day { get; set; }

        /// <summary>
        /// Current minute
        /// </summary>
        public int Hour { get; set; }

        public TimeOfDay()
        {
            Hour = 1;
            Day = 1;
            Month = 1;
            Year = 1;
        }

        public TimeOfDay(IChronology baseChronology)
        {
            Hour = 1;
            Day = 1;
            Month = 1;

            BaseChronology = baseChronology;

            Year = BaseChronology.StartingYear;
        }

        /// <summary>
        /// The name of the month
        /// </summary>
        public string MonthName()
        {
            if (BaseChronology?.Months == null)
                return "Month";

            var monthName = BaseChronology.Months.FirstOrDefault();

            if (Month <= BaseChronology.Months.Count())
                monthName = BaseChronology.Months.ElementAt(Month);

            return monthName;
        }


        /// <summary>
        /// Advance time by one hour
        /// </summary>
        public void AdvanceByHour()
        {
            Hour++;

            if(Hour > BaseChronology.HoursPerDay)
            {
                Hour = 1;
                Day++;
            }

            if(Day > BaseChronology.DaysPerMonth)
            {
                Day = 1;
                Month++;
            }

            if(Month > BaseChronology.Months.Count())
            {
                Month = 1;
                Year++;
            }
        }

        /// <summary>
        /// Set the time to something specific
        /// </summary>
        public void Set(int year, int month, int day, int hour)
        {
            Year = year;
            Month = month;
            Day = day;
            Hour = hour;

            if (Hour > BaseChronology.HoursPerDay || Hour <= 0)
                Hour = 1;

            if (Day > BaseChronology.DaysPerMonth || Day <= 0)
                Day = 1;

            if (Month > BaseChronology.Months.Count() || Month <= 0)
                Month = 1;

            if (Year < BaseChronology.StartingYear)
                Year = BaseChronology.StartingYear;
        }
    }
}
