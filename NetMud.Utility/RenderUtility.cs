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
        /// Make the first letter caps, the rest lower
        /// </summary>
        /// <param name="value">the string to caps</param>
        /// <returns>String</returns>
        public static string ProperCaps(this string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            string[] words = value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            return string.Join(" ", words.Select(word => word.CapsFirstLetter()));
        }

        /// <summary>
        /// Converts the string to uppercase for the first letter
        /// </summary>
        /// <param name="value"></param>
        /// <param name="onlyFirstLetter"></param>
        /// <returns></returns>
        public static string CapsFirstLetter(this string value, bool onlyFirstLetter = false)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            if (value.Length == 1)
            {
                return value.ToUpperInvariant();
            }

            string firstLetter = value.Substring(0, 1).ToUpperInvariant();
            string wordRemainder = onlyFirstLetter ? value.Substring(1) : value.Substring(1).ToLowerInvariant();

            return string.Format("{0}{1}", firstLetter, wordRemainder);
        }

        /// <summary>
        /// Split a string with a single delimiter
        /// </summary>
        /// <param name="value">the string being split</param>
        /// <param name="delimiter">the single delimiter</param>
        /// <returns>a string array</returns>
        public static string[] Split(this string value, string delimiter, StringSplitOptions splitOpts)
        {
            if (value == null)
            {
                return new string[0];
            }

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
            if (value == null)
            {
                return new string[0];
            }

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
            {
                for (int y = 0; y < arr.GetLength(1); y++)
                {
                    for (int z = 0; z < arr.GetLength(2); z++)
                    {
                        arr[x, y, z] = value;
                    }
                }
            }

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
            {
                for (int y = 0; y < arr.GetLength(1); y++)
                {
                    arr[x, y] = value;
                }
            }

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
            {
                arr[x] = value;
            }

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
                {
                    str = padString + str;
                }
                else
                {
                    str += padString;
                }

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
            {
                return string.Empty;
            }

            if (stringList.Length == 1)
            {
                return stringList[0];
            }

            //Remove any prior punctuated things
            return string.Join(". ", stringList).Replace(".. ", ". ").Replace("?. ", "? ").Replace("!. ", "! ");
        }

        /// <summary>
        /// Render this collection of strings as a commalist (appends punctuation)
        /// </summary>
        /// <param name="stringList">The string list to join</param>
        /// <param name="mode">The punctuation mode</param>
        /// <returns>A commalist format string</returns>
        public static string CommaList(this IEnumerable<string> stringList, SplitListType mode = SplitListType.AllComma)
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
            {
                return string.Empty;
            }

            if (stringList.Length == 1)
            {
                return stringList[0];
            }

            string returnString = string.Empty;
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
                    int lastComma = returnString.LastIndexOf(',');

                    returnString = string.Format("{0} and {1}", returnString.Substring(0, lastComma), returnString.Substring(lastComma + 1));
                    break;
                case SplitListType.OxfordComma:
                    returnString = string.Join(", ", stringList);
                    int lastOxfordComma = returnString.LastIndexOf(',');

                    if (stringList.Count() == 2)
                    {
                        returnString = string.Format("{0} and {1}", returnString.Substring(0, lastOxfordComma), returnString.Substring(lastOxfordComma + 1));
                    }
                    else
                    {
                        returnString = string.Format("{0}, and {1}", returnString.Substring(0, lastOxfordComma), returnString.Substring(lastOxfordComma + 1));
                    }
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
            try
            {
                Type type = enumVal.GetType();
                System.Reflection.MemberInfo[] memInfo = type.GetMember(enumVal.ToString());

                object[] attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

                //Is there a description? Otherwise to-string it.
                return attributes.Length > 0
                        ? ((DescriptionAttribute)attributes[0]).Description
                        : enumVal.ToString();
            }
            catch
            {
                //Don't want to barf on this stuff
            }

            return string.Empty;
        }

        /// <summary>
        /// Get the greek alphabet equivelant of an integer
        /// </summary>
        /// <param name="iterator">the integer in question</param>
        /// <param name="specialCharacter">should this be the actual letter (true) or the english word equivelant (false)</param>
        /// <returns>A string, only works 1-24</returns>
        public static string ToGreek(this int iterator, bool specialCharacter = false)
        {
            string returnValue = "";
            switch (iterator)
            {
                case 1:
                    if (specialCharacter)
                    {
                        returnValue = "alpha";
                    }
                    else
                    {
                        returnValue = "α";
                    }

                    break;
                case 2:
                    if (specialCharacter)
                    {
                        returnValue = "beta";
                    }
                    else
                    {
                        returnValue = "β";
                    }

                    break;
                case 3:
                    if (specialCharacter)
                    {
                        returnValue = "gamma";
                    }
                    else
                    {
                        returnValue = "γ";
                    }

                    break;
                case 4:
                    if (specialCharacter)
                    {
                        returnValue = "delta";
                    }
                    else
                    {
                        returnValue = "δ";
                    }

                    break;
                case 5:
                    if (specialCharacter)
                    {
                        returnValue = "epsilon";
                    }
                    else
                    {
                        returnValue = "ε";
                    }

                    break;
                case 6:
                    if (specialCharacter)
                    {
                        returnValue = "zeta";
                    }
                    else
                    {
                        returnValue = "ζ";
                    }

                    break;
                case 7:
                    if (specialCharacter)
                    {
                        returnValue = "eta";
                    }
                    else
                    {
                        returnValue = "η";
                    }

                    break;
                case 8:
                    if (specialCharacter)
                    {
                        returnValue = "theta";
                    }
                    else
                    {
                        returnValue = "θ";
                    }

                    break;
                case 9:
                    if (specialCharacter)
                    {
                        returnValue = "iota";
                    }
                    else
                    {
                        returnValue = "ι";
                    }

                    break;
                case 10:
                    if (specialCharacter)
                    {
                        returnValue = "kappa";
                    }
                    else
                    {
                        returnValue = "κ";
                    }

                    break;
                case 11:
                    if (specialCharacter)
                    {
                        returnValue = "lamda";
                    }
                    else
                    {
                        returnValue = "λ";
                    }

                    break;
                case 12:
                    if (specialCharacter)
                    {
                        returnValue = "mu";
                    }
                    else
                    {
                        returnValue = "μ";
                    }

                    break;
                case 13:
                    if (specialCharacter)
                    {
                        returnValue = "nu";
                    }
                    else
                    {
                        returnValue = "ν";
                    }

                    break;
                case 14:
                    if (specialCharacter)
                    {
                        returnValue = "xi";
                    }
                    else
                    {
                        returnValue = "ξ";
                    }

                    break;
                case 15:
                    if (specialCharacter)
                    {
                        returnValue = "omicron";
                    }
                    else
                    {
                        returnValue = "ο";
                    }

                    break;
                case 16:
                    if (specialCharacter)
                    {
                        returnValue = "pi";
                    }
                    else
                    {
                        returnValue = "π";
                    }

                    break;
                case 17:
                    if (specialCharacter)
                    {
                        returnValue = "rho";
                    }
                    else
                    {
                        returnValue = "ρ";
                    }

                    break;
                case 18:
                    if (specialCharacter)
                    {
                        returnValue = "sigma";
                    }
                    else
                    {
                        returnValue = "σ";
                    }

                    break;
                case 19:
                    if (specialCharacter)
                    {
                        returnValue = "tau";
                    }
                    else
                    {
                        returnValue = "τ";
                    }

                    break;
                case 20:
                    if (specialCharacter)
                    {
                        returnValue = "upsilon";
                    }
                    else
                    {
                        returnValue = "υ";
                    }

                    break;
                case 21:
                    if (specialCharacter)
                    {
                        returnValue = "phi";
                    }
                    else
                    {
                        returnValue = "φ";
                    }

                    break;
                case 22:
                    if (specialCharacter)
                    {
                        returnValue = "chi";
                    }
                    else
                    {
                        returnValue = "χ";
                    }

                    break;
                case 23:
                    if (specialCharacter)
                    {
                        returnValue = "psi";
                    }
                    else
                    {
                        returnValue = "ψ";
                    }

                    break;
                case 24:
                    if (specialCharacter)
                    {
                        returnValue = "omega";
                    }
                    else
                    {
                        returnValue = "ω";
                    }

                    break;
            }

            return returnValue;
        }
        #endregion
    }
}
