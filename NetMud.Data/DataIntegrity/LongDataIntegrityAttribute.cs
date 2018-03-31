using System;

namespace NetMud.Data.DataIntegrity
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class LongDataIntegrityAttribute : BaseDataIntegrity
    {
        /// <summary>
        /// Lower value for range. Is a greater than not a greater or equals
        /// </summary>
        public long LowerBound { get; private set; }

        /// <summary>
        /// Upper value for range. Is a less than not a less or equals
        /// </summary>
        public long UpperBound { get; private set; }

        /// <summary>
        /// How to check against this result; returns true if it passes Longegrity
        /// </summary>
        internal override bool Verify(object val)
        {
            long value = Utility.DataUtility.TryConvert<long>(val);

            return value > LowerBound && value < UpperBound;
        }

        /// <summary>
        /// Creates a data Longegrity attribute
        /// </summary>
        /// <param name="errorMessage"></param>
        /// <param name="warning">Not a required field but will display on the editor itself</param>
        public LongDataIntegrityAttribute(string errorMessage, long lowerBound = long.MinValue, long upperBound = long.MaxValue, bool warning = false) : base(errorMessage, warning)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;
        }
    }
}
