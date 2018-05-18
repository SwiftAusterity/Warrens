using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace NetMud.Utility
{
    /// <summary>
    /// Utilities for rendering output
    /// </summary>
    public static partial class RenderUtility
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

        public static string CommaList(this IEnumerable<string> stringList, SplitListType mode)
        {
            return stringList.ToArray().CommaList(mode);
        }

        public static string CommaList(this string[] stringList, SplitListType mode)
        {
            if (stringList.Length == 0)
                return string.Empty;

            if (stringList.Length == 1)
                return stringList[0];

            var returnString = string.Empty;
            switch (mode)
            {
                case SplitListType.AllAnd:
                    returnString = String.Join(" and ", stringList);
                    break;
                case SplitListType.AllComma:
                    returnString = String.Join(", ", stringList);
                    break;
                case SplitListType.CommaWithAnd:
                    returnString = String.Join(", ", stringList);
                    var lastComma = returnString.LastIndexOf(',');

                    returnString = String.Format("{0} and {1}", returnString.Substring(0, lastComma), returnString.Substring(lastComma + 1));
                    break;
                case SplitListType.OxfordComma:
                    returnString = String.Join(", ", stringList);
                    var lastOxfordComma = returnString.LastIndexOf(',');

                    returnString = String.Format("{0}, and {1}", returnString.Substring(0, lastOxfordComma), returnString.Substring(lastOxfordComma + 1));
                    break;
            }

            return returnString;
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
