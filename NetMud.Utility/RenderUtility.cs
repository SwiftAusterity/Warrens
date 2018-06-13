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
        /// Split a string with a single delimiter
        /// </summary>
        /// <param name="value">the string being split</param>
        /// <param name="delimiter">the single delimiter</param>
        /// <returns>a string array</returns>
        public static string[] Split(this string value, string delimiter, StringSplitOptions splitOpts)
        {
            return value.Split(new string[] { delimiter }, splitOpts);
        }

        /// <summary>
        /// Split a string with a single delimiter
        /// </summary>
        /// <param name="value">the string being split</param>
        /// <param name="delimiter">the single delimiter</param>
        /// <returns>a string array</returns>
        public static string[] Split(this string value, char delimiter, StringSplitOptions splitOpts)
        {
            return value.Split(new char[] { delimiter }, splitOpts);
        }

        /// <summary>
        /// Populate an array with a single value
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="arr">The array, this is an ext method</param>
        /// <param name="value">The value to cram</param>
        public static T[,,] Populate<T>(this T[,,] arr, T value)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
                for (int y = 0; y < arr.GetLength(1); y++)
                    for (int z = 0; z < arr.GetLength(2); z++)
                        arr[x, y, z] = value;

            return arr;
        }

        /// <summary>
        /// Populate an array with a single value
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="arr">The array, this is an ext method</param>
        /// <param name="value">The value to cram</param>
        public static T[,] Populate<T>(this T[,] arr, T value)
        {
            for (int x = 0; x < arr.GetLength(0); x++)
                for (int y = 0; y < arr.GetLength(1); y++)
                    arr[x, y] = value;

            return arr;
        }

        /// <summary>
        /// Populate an array with a single value
        /// </summary>
        /// <typeparam name="T">The array type</typeparam>
        /// <param name="arr">The array, this is an ext method</param>
        /// <param name="value">The value to cram</param>
        public static T[] Populate<T>(this T[] arr, T value)
        {
            for (int x = 0; x < arr.Length; x++)
                arr[x] = value;

            return arr;
        }

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
        /// Render this collection of strings as a paragraph. (appends punctuation)
        /// </summary>
        /// <param name="stringList">The string list to join</param>
        /// <returns>A paragraph format string</returns>
        public static string ParagraphList(this IEnumerable<string> stringList)
        {
            return stringList.ToArray().ParagraphList();
        }

        /// <summary>
        /// Render this collection of strings as a paragraph. (appends punctuation)
        /// </summary>
        /// <param name="stringList">The string list to join</param>
        /// <returns>A paragraph format string</returns>
        public static string ParagraphList(this string[] stringList)
        {
            if (stringList.Length == 0)
                return string.Empty;

            if (stringList.Length == 1)
                return stringList[0];

            //Remove any prior punctuated things
            return string.Join(". ", stringList).Replace(".. ", ". ").Replace("?. ", "? ").Replace("!. ", "! ");
        }

        /// <summary>
        /// Render this collection of strings as a commalist (appends punctuation)
        /// </summary>
        /// <param name="stringList">The string list to join</param>
        /// <param name="mode">The punctuation mode</param>
        /// <returns>A commalist format string</returns>
        public static string CommaList(this IEnumerable<string> stringList, SplitListType mode)
        {
            return stringList.ToArray().CommaList(mode);
        }

        /// <summary>
        /// Render this collection of strings as a commalist (appends punctuation)
        /// </summary>
        /// <param name="stringList">The string list to join</param>
        /// <param name="mode">The punctuation mode</param>
        /// <returns>A commalist format string</returns>
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
                    returnString = string.Join(" and ", stringList);
                    break;
                case SplitListType.AllComma:
                    returnString = string.Join(", ", stringList);
                    break;
                case SplitListType.CommaWithAnd:
                    returnString = string.Join(", ", stringList);
                    var lastComma = returnString.LastIndexOf(',');

                    returnString = string.Format("{0} and {1}", returnString.Substring(0, lastComma), returnString.Substring(lastComma + 1));
                    break;
                case SplitListType.OxfordComma:
                    returnString = string.Join(", ", stringList);
                    var lastOxfordComma = returnString.LastIndexOf(',');

                    returnString = string.Format("{0}, and {1}", returnString.Substring(0, lastOxfordComma), returnString.Substring(lastOxfordComma + 1));
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
