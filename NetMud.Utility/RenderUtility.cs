using System;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Utility
{
    /// <summary>
    /// Utilities for rendering output
    /// </summary>
    public static class RenderUtility
    {
        //TODO: Define "output target type" for rendering to can capsule elements and bumpers
        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        private const string encapsulationElement = "p";
        /// <summary>
        /// Adding a "new line" to the output
        /// </summary>
        private const string bumperElement = "<br />";

        /// <summary>
        /// Encapsulate output lines for display to a client
        /// </summary>
        /// <param name="lines">the text lines to encapsulate</param>
        /// <returns>a single string blob of all the output encapsulated</returns>
        public static string EncapsulateOutput(IEnumerable<string> lines)
        {
            var returnString = new StringBuilder();

            foreach (var line in lines)
            {
                if (!string.IsNullOrWhiteSpace(line))
                    returnString.AppendFormat("<{0}>{1}</{0}>", encapsulationElement, line);
                else
                    returnString.Append(bumperElement); //blank strings mean carriage returns
            }

            return returnString.ToString();
        }

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
        /// Encapsulates a string for output to a client
        /// </summary>
        /// <param name="str">the string to encapsulate</param>
        /// <returns>the encapsulated output</returns>
        public static string EncapsulateOutput(this string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return String.Format("<{0}>{1}</{0}>", encapsulationElement, str);
            else
                return bumperElement; //blank strings mean carriage returns
        }
        #endregion

    }
}
