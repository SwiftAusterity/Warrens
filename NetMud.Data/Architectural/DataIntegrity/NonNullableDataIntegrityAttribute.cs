using System;

namespace NetMud.Data.Architectural.DataIntegrity
{
    /// <summary>
    /// Field must not be null (doesn't allow multiple as.. why would you need that)
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class NonNullableDataIntegrityAttribute : BaseDataIntegrity
    {

        /// <summary>
        /// How to check against this result; returns true if it passes Longegrity
        /// </summary>
        internal override bool Verify(object val)
        {
            if (val?.GetType() == typeof(string))
                return !string.IsNullOrEmpty((string)val);

            return val != null;
        }

        /// <summary>
        /// Creates an attribute
        /// </summary>
        /// <param name="errorMessage">error to display when this fails the integrity check</param>
        /// <param name="warning">Not a required field but will display on the editor itself</param>
        public NonNullableDataIntegrityAttribute(string errorMessage) : base(errorMessage, true)
        {
        }
    }
}
