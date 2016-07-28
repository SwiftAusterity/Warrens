using System.Collections.Generic;

namespace NetMud.DataStructure.Base.System
{
    /// <summary>
    /// Types of communication channels
    /// </summary>
    public interface IChannelType
    {
        /// <summary>
        /// The format the cachekeys for the comms objects take
        /// </summary>
        string cacheKeyFormat { get; }

        /// <summary>
        /// What type of connection the player has
        /// </summary>
        ConnectionType ConnectedBy { get; }
        
        /// <summary>
        /// Encapsulate output lines for display to a client
        /// </summary>
        /// <param name="lines">the text lines to encapsulate</param>
        /// <returns>a single string blob of all the output encapsulated</returns>
        string EncapsulateOutput(IEnumerable<string> lines);

        /// <summary>
        /// Encapsulates a string for output to a client
        /// </summary>
        /// <param name="str">the string to encapsulate</param>
        /// <returns>the encapsulated output</returns>
        string EncapsulateOutput(string str);

        /// <summary>
        /// Reformats strings replacing style formatting codes
        /// </summary>
        /// <param name="originalString">The original message unaltered</param>
        /// <param name="formatToReplace">The internally recognized format</param>
        /// <param name="styleElement">the thing to replace the codes with</param>
        /// <returns>A reformatted string</returns>
        bool ReplaceColor(SupportedColors styleType, string colorGlyph, ref string originalString);

        /// <summary>
        /// The list of accepted color codes and what they translate to for color translation
        /// </summary>
        Dictionary<SupportedColors, string> SupportedColorTranslations { get; }
    }
}
