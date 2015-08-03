using System;
using System.Text.RegularExpressions;

namespace NutMud.Commands.Attributes
{
    /// <summary>
    /// Restriction paramater for command methods that detail the command's parameter types, keywords, patterns, etc.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class CommandParameterAttribute : Attribute
    {
        /// <summary>
        /// How the parameter is used in the command
        /// </summary>
        public CommandUsage Usage { get; private set; }

        /// <summary>
        /// The system type of the parameter expected
        /// </summary>
        public Type ParameterType { get; private set; }

        /// <summary>
        /// How this parameter can be found (is it data, is it a live entity)
        /// </summary>
        public CacheReferenceType[] CacheTypes { get; private set; }

        /// <summary>
        /// Does this pattern keyword follow a regex pattern
        /// </summary>
        public string RegExPattern { get; private set; }

        /// <summary>
        /// Is this parameter optional
        /// </summary>
        public bool Optional { get; private set; }

        /// <summary>
        /// Creates a new parameter attribute
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="matchingPattern">Does this pattern keyword follow a regex pattern</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, string matchingPattern, bool optional)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
            RegExPattern = matchingPattern;
            Optional = optional;
        }

        /// <summary>
        /// Creates a new parameter attribute with a blank matching pattern
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, bool optional)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
            RegExPattern = string.Empty;
            Optional = optional;
        }

        /// <summary>
        /// Does the keywords incoming match the regex pattern (unless it's blank, in which case it always matches)
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public bool MatchesPattern(string inputString)
        {
            return string.IsNullOrWhiteSpace(RegExPattern) || Regex.IsMatch(inputString, RegExPattern);
        }
    }

    /// <summary>
    /// What does this command want to do with this parameter
    /// </summary>
    public enum CommandUsage : short
    {
        Subject = 0,
        Target = 1,
        Supporting = 2
    }

    /// <summary>
    /// How this parameter can be found (is it data, is it a live entity)
    /// </summary>
    public enum CacheReferenceType
    {
        Entity,
        Reference,
        Code,
        Container,
        Data,
        Help //hacky add for help specifically
    }
}
