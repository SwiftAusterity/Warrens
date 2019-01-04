using System;

namespace NetMud.Data.Architectural.DataIntegrity
{
    /// <summary>
    /// Details what keywords match a command
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public abstract class BaseDataIntegrity : Attribute
    {
        /// <summary>
        /// The error message to add to the error list
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Not a required field but will display on the editor itself
        /// </summary>
        public bool Warning { get; private set; }

        /// <summary>
        /// How to check against this result
        /// </summary>
        internal abstract bool Verify(object val);

        /// <summary>
        /// Creates a data integrity attribute
        /// </summary>
        /// <param name="errorMessage">error to display when this fails the integrity check</param>
        /// <param name="warning">Not a required field but will display on the editor itself</param>
        public BaseDataIntegrity(string errorMessage, bool warning = false)
        {
            if (string.IsNullOrWhiteSpace(errorMessage))
                throw (new ArgumentNullException(string.Format("{0} data integrity error message blank on implimentation.", GetType().ToString())));
            
            ErrorMessage = errorMessage;
            Warning = warning;
        }
    }
}
