namespace NetMud.DataStructure.Base.World
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
        int Minute { get; set; }
    }
}
