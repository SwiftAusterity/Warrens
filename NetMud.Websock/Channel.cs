using NetMud.DataStructure.Base.System;
using System;
using System.Collections.Generic;
using System.Text;

namespace NetMud.Websock
{
    /// <summary>
    /// Partial class for websocket channel details
    /// </summary>
    public abstract class Channel : IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        public string cacheKeyFormat { get { return "LiveWebSocket.{0}"; } }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        public ConnectionType ConnectedBy { get { return ConnectionType.Websocket; } }

        /// <summary>
        /// Encapsulation element for rendering to html
        /// </summary>
        private const string EncapsulationElement = "p";

        /// <summary>
        /// Adding a "new line" to the output 
        /// </summary>
        private const string BumperElement = "<br />";

        /// <summary>
        /// List of style types to element
        /// </summary>
        private Dictionary<SupportedColors, string> _colors = new Dictionary<SupportedColors, string> 
        {
            { SupportedColors.Bold,         "font-weight: bold;" },
            { SupportedColors.Italics,      "font-weight: italic;" },
            { SupportedColors.Blue,         "color: #0000FF;" },
            { SupportedColors.LightBlue,    "color: #6495ED;" },
            { SupportedColors.Orange,       "color: #FF7F50;" },
            { SupportedColors.LightOrange,  "color: #D2691E;" },
            { SupportedColors.Yellow,       "color: #FFD700;" },
            { SupportedColors.LightYellow,  "color: #F0E68C;" },
            { SupportedColors.Green,        "color: #008000;" },
            { SupportedColors.LightGreen,   "color: #90EE90;" },
            { SupportedColors.Indigo,       "color: #4B0082;" },
            { SupportedColors.LightPurple,  "color: #9400D3;" },
            { SupportedColors.Red,          "color: #FF4500;" },
            { SupportedColors.LightRed,     "color: #800000;" },
            { SupportedColors.Pink,         "color: #FF69B4;" },
            { SupportedColors.LightPink,    "color: #FFB6C1;" }
        };

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

        /// <summary>
        /// Engine for translating output text with color codes into proper output
        /// </summary>
        /// <param name="message">the text to translate</param>
        /// <returns>translated text</returns>
        public bool ReplaceColor(SupportedColors styleType, string formatToReplace, ref string originalString)
        {
            //If the origin string or formatting is blank just return the orginal string
            if (string.IsNullOrWhiteSpace(originalString) || string.IsNullOrWhiteSpace(formatToReplace))
                return false;

            var styleElement = SupportedColorTranslations[styleType];

            //If the destination string is blank, just remove them all since they'd come back empty anyways
            if (string.IsNullOrWhiteSpace(styleElement))
                originalString = originalString.Replace(formatToReplace, string.Empty);
            else
            {
                var firstIndex = originalString.IndexOf(formatToReplace);

                if (firstIndex < 0)
                    return false;
                else
                {
                    var secondIndex = originalString.IndexOf(formatToReplace, firstIndex + formatToReplace.Length);

                    //Yes 1st instance but no second instance? replace them all with empty string to scrub the string.
                    if (secondIndex < 0)
                        originalString = originalString.Replace(formatToReplace, string.Empty);
                    else
                    {
                        var lengthToSkip = formatToReplace.Length;

                        originalString = string.Format("{0}<span style=\"{3}\">{1}</span>{2}"
                            , firstIndex == 0 ? string.Empty : originalString.Substring(0, firstIndex)
                            , originalString.Substring(firstIndex + lengthToSkip, secondIndex - firstIndex - lengthToSkip)
                            , originalString.Substring(secondIndex + lengthToSkip)
                            , styleElement);
                    }
                }
            }

            return true;
        }

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
                return string.Format("<{0}>{1}</{0}>", EncapsulationElement, str);
            else
                return BumperElement; //blank strings mean carriage returns
        }
    }
}
