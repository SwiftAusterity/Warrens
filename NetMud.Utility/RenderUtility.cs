using System;
using System.ComponentModel;
namespace NetMud.Utility
{
    /// <summary>
    /// Utilities for rendering output
    /// </summary>
    public static class RenderUtility
    {
        #region Extensions
        /// <summary>
        /// Pads a string with characters
        /// </summary>
        /// <param name="str">the string to pad</param>
        /// <param name="padAmount">how many to pad with</param>
        /// <param name="padString">what you're padding with</param>
        /// <param name="toTheLeft">is this to the left or right</param>
        /// <returns>the padded string</returns>
        public static string PadWithString(this string str, int padAmount, string padString, bool toTheLeft)
        {
            while (padAmount > 0)
            {
                if (toTheLeft)
                    str = padString + str;
                else
                    str = str + padString;

                padAmount--;
            }

            return str;
        }

        /// <summary>
        /// Gets an attribute on an enum field value
        /// </summary>
        /// <param name="enumVal">The enum value</param>
        /// <returns>The description attribute of the enum value</returns>
        /// <example>string desc = myEnumVariable.GetDescription();</example>
        public static string GetDescription(this Enum enumVal)
        {
            var type = enumVal.GetType();
            var memInfo = type.GetMember(enumVal.ToString());

            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            return attributes.Length > 0 
                    ? ((DescriptionAttribute)attributes[0]).Description 
                    : string.Empty;
        }
        #endregion
    }
}
