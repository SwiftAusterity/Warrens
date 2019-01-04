namespace NetMud.DataStructure.Gaia
{
    /// <summary>
    /// An instance of time for the mud
    /// </summary>
    public interface ITimeOfDay
    {
        /// <summary>
        /// Current month
        /// </summary>
        int Month { get; set; }

        /// <summary>
        /// Current year
        /// </summary>
        int Year { get; set; }

        /// <summary>
        /// Current day
        /// </summary>
        int Day { get; set; }

        /// <summary>
        /// Current minute
        /// </summary>
        int Hour { get; set; }

        /// <summary>
        /// The name of the month
        /// </summary>
        string MonthName();

        /// <summary>
        /// Advance time by one hour
        /// </summary>
        void AdvanceByHour();

        /// <summary>
        /// Set the time to something specific
        /// </summary>
        void Set(int year, int month, int day, int hour);
    }
}
