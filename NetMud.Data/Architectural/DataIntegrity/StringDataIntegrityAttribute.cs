using NetMud.DataStructure.Architectural;
using System;

namespace NetMud.Data.Architectural.DataIntegrity
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class StringDataIntegrityAttribute : BaseDataIntegrity
    {
        /// <summary>
        /// Min length for the string; Is a greater than or equal to.
        /// </summary>
        public int MinimumLength { get; private set; }

        /// <summary>
        /// Min length for the string; Is a less than or equal to
        /// </summary>
        public int MaximumLength { get; private set; }

        /// <summary>
        /// How to check against this result; returns true if it passes integrity
        /// </summary>
        internal override bool Verify(object val)
        {
            string compareValue = "";

            if (val != null && val.GetType() == typeof(MarkdownString))
            {
                MarkdownString mdString = Utility.DataUtility.TryConvert<MarkdownString>(val);

                if (!MarkdownString.IsNullOrWhiteSpace(mdString))
                {
                    compareValue = mdString.Value;
                }
            }
            else
            {
                compareValue = Utility.DataUtility.TryConvert<string>(val);
            }

            return !string.IsNullOrWhiteSpace(compareValue) && compareValue.Length >= MinimumLength && compareValue.Length <= MaximumLength;
        }

        /// <summary>
        /// Creates a data integrity attribute
        /// </summary>
        /// <param name="errorMessage">error to display when this fails the integrity check</param>
        /// <param name="warning">Not a required field but will display on the editor itself</param>
        public StringDataIntegrityAttribute(string errorMessage, int minimumLength = 0, int maximumLength = 9999, bool warning = false) : base(errorMessage, warning)
        {
            MinimumLength = minimumLength;
            MaximumLength = maximumLength;
        }
    }
}
