using NetMud.DataStructure.System;
using System.Collections.Generic;

namespace NetMud.Communication
{
    public class InternalChannel : IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat
        {
            get {  return string.Empty; }
        }

        /// <summary>
        /// List of style types to element
        /// </summary>
        private readonly Dictionary<SupportedColors, string> _colors = new Dictionary<SupportedColors, string> 
        {
            { SupportedColors.Bold,         string.Empty },
            { SupportedColors.Italics,      string.Empty },
            { SupportedColors.Blue,         string.Empty },
            { SupportedColors.LightBlue,    string.Empty },
            { SupportedColors.Orange,       string.Empty },
            { SupportedColors.LightOrange,  string.Empty },
            { SupportedColors.Yellow,       string.Empty },
            { SupportedColors.LightYellow,  string.Empty },
            { SupportedColors.Green,        string.Empty },
            { SupportedColors.LightGreen,   string.Empty },
            { SupportedColors.Indigo,       string.Empty },
            { SupportedColors.LightPurple,  string.Empty },
            { SupportedColors.Red,          string.Empty },
            { SupportedColors.LightRed,     string.Empty },
            { SupportedColors.Pink,         string.Empty },
            { SupportedColors.LightPink,    string.Empty }
        };

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy
        {
            get { return ConnectionType.Internal; }
        }

        /// <summary>
        /// Encapsulate output lines for display to a client
        /// </summary>
        /// <param name="lines">the text lines to encapsulate</param>
        /// <returns>a single string blob of all the output encapsulated</returns>
        public string EncapsulateOutput(IEnumerable<string> lines)
        {
            //We're not doing any output encapsulation for internal guys
            return string.Join(" ", lines);
        }

        /// <summary>
        /// Encapsulates a string for output to a client
        /// </summary>
        /// <param name="str">the string to encapsulate</param>
        /// <returns>the encapsulated output</returns>
        public string EncapsulateOutput(string str)
        {
            //We're not doing any output encapsulation for internal guys
            return str;
        }

        public bool ReplaceColor(SupportedColors styleType, string formatToReplace, ref string originalString)
        {
            //If the origin string or formatting is blank just return the orginal string
            if (string.IsNullOrWhiteSpace(originalString))
            {
                return false;
            }

            //It's all blank anyways
            originalString = originalString.Replace(formatToReplace, string.Empty);

            return true;
        }

        /// <summary>
        /// Returns a list of all supported systems colors and what colors they can become
        /// </summary>
        public Dictionary<SupportedColors, string> SupportedColorTranslations
        {
            get
            {
                return _colors;
            }
        }
    }
}
