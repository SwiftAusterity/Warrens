using NetMud.DataStructure.Base.World;

namespace NetMud.Data.System
{
    /// <summary>
    /// An instance of time for the mud
    /// </summary>
    public class TimeOfDay : ITimeOfDay
    {
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
        public int Minute { get; set; }
    }
}
