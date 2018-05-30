using NetMud.DataStructure.Base.System;

using System;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Telnet
{
    /// <summary>
    /// Partial class for telnet channel details
    /// </summary>
    public abstract class Channel : IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat { get { return "LiveTelnet.{0}"; } }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy { get { return ConnectionType.Telnet; } }

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
        private const string BumperElement = "\n\r";

        /// <summary>
        /// Encapsulate output lines for display to a client
        /// </summary>
        /// <param name="lines">the text lines to encapsulate</param>
        /// <returns>a single string blob of all the output encapsulated</returns>
        public string EncapsulateOutput(IEnumerable<string> lines)
        {
            var returnString = new StringBuilder();

            foreach (var line in lines)
                returnString.AppendFormat(EncapsulateOutput(line));

            return returnString.ToString();
        }

        /// <summary>
        /// Encapsulates a string for output to a client
        /// </summary>
        /// <param name="str">the string to encapsulate</param>
        /// <returns>the encapsulated output</returns>
        public string EncapsulateOutput(string str)
        {
            if (!string.IsNullOrWhiteSpace(str))
                return string.Format("{1}{0}", BumperElement, str);
            else
                return BumperElement; //blank strings mean carriage returns
        }

        private Dictionary<SupportedColors, string> _colors = new Dictionary<SupportedColors, string> 
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


        public Dictionary<SupportedColors, string> SupportedColorTranslations
        {
            get { return _colors; }
        }


        public bool ReplaceColor(SupportedColors styleType, string formatToReplace, ref string originalString)
        {
            throw new NotImplementedException();
        }
    }
}
