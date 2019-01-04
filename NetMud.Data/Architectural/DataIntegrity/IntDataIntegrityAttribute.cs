using System;

namespace NetMud.Data.Architectural.DataIntegrity
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IntDataIntegrityAttribute : BaseDataIntegrity
    {
        /// <summary>
        /// Lower value for range. Is a greater than not a greater or equals
        /// </summary>
        public int LowerBound { get; private set; }

        /// <summary>
        /// Upper value for range. Is a less than not a less or equals
        /// </summary>
        public int UpperBound { get; private set; }

        /// <summary>
        /// How to check against this result; returns true if it passes integrity
        /// </summary>
        internal override bool Verify(object val)
        {
            int value = Utility.DataUtility.TryConvert<int>(val);

            return value >= LowerBound && value <= UpperBound;
        }

        /// <summary>
        /// Creates a data integrity attribute
        /// </summary>
        /// <param name="errorMessage">error to display when this fails the integrity check</param>
        /// <param name="warning">Not a required field but will display on the editor itself</param>
        public IntDataIntegrityAttribute(string errorMessage, int lowerBound = int.MinValue, int upperBound = int.MaxValue, bool warning = false) : base(errorMessage, warning)
        {
            UpperBound = upperBound;
            LowerBound = lowerBound;
        }
    }
}
