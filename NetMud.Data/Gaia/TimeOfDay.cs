using NetMud.DataStructure.Gaia;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace NetMud.Data.Gaia
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
        [Display(Name = "Month", Description = "What month it currently is.")]
        [DataType(DataType.Text)]
        public int Month { get; set; }

        /// <summary>
        /// Current year
        /// </summary>
        [Display(Name = "Year", Description = "What year it currently is.")]
        [DataType(DataType.Text)]
        public int Year { get; set; }

        /// <summary>
        /// Current day
        /// </summary>
        [Display(Name = "Day", Description = "What day it currently is.")]
        [DataType(DataType.Text)]
        public int Day { get; set; }

        /// <summary>
        /// Current minute
        /// </summary>
        [Display(Name = "Hour", Description = "What hour of the day it currently is.")]
        [DataType(DataType.Text)]
        public int Hour { get; set; }

        public TimeOfDay()
        {
            Hour = 1;
            Day = 1;
            Month = 1;
            Year = 1;

            BaseChronology = new Chronology();
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
            {
                return "Month";
            }

            string monthName = BaseChronology.Months.FirstOrDefault();

            if (Month <= BaseChronology.Months.Count())
            {
                monthName = BaseChronology.Months.ElementAt(Month);
            }

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
            {
                Hour = 1;
            }

            if (Day > BaseChronology.DaysPerMonth || Day <= 0)
            {
                Day = 1;
            }

            if (Month > BaseChronology.Months.Count() || Month <= 0)
            {
                Month = 1;
            }

            if (Year < BaseChronology.StartingYear)
            {
                Year = BaseChronology.StartingYear;
            }
        }
    }
}
