using System;
using System.Text.RegularExpressions;

namespace NetMud.Commands.Attributes
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
        public Type[] ParameterTypes { get; private set; }

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
        /// Does this parameter require the prior one (Subject->Target->Supporting)
        /// </summary>
        public bool RequiresPreviousParameter { get; private set; }

        /// <summary>
        /// Creates a new parameter attribute
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheType">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="matchingPattern">Does this pattern keyword follow a regex pattern</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType cacheType, string matchingPattern, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = new Type[] { type };
            CacheTypes = new CacheReferenceType[] { cacheType };
            RegExPattern = matchingPattern;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute with a blank matching pattern
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheType">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType cacheType, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = new Type[] { type };
            CacheTypes = new CacheReferenceType[] { cacheType };
            RegExPattern = string.Empty;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="matchingPattern">Does this pattern keyword follow a regex pattern</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, string matchingPattern, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = new Type[] { type };
            CacheTypes = cacheTypes;
            RegExPattern = matchingPattern;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute with a blank matching pattern
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = new Type[] { type };
            CacheTypes = cacheTypes;
            RegExPattern = string.Empty;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheType">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="matchingPattern">Does this pattern keyword follow a regex pattern</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type[] types, CacheReferenceType cacheType, string matchingPattern, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = types;
            CacheTypes = new CacheReferenceType[] { cacheType };
            RegExPattern = matchingPattern;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute with a blank matching pattern
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheType">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type[] types, CacheReferenceType cacheType, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = types;
            CacheTypes = new CacheReferenceType[] { cacheType };
            RegExPattern = string.Empty;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="matchingPattern">Does this pattern keyword follow a regex pattern</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type[] types, CacheReferenceType[] cacheTypes, string matchingPattern, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = types;
            CacheTypes = cacheTypes;
            RegExPattern = matchingPattern;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Creates a new parameter attribute with a blank matching pattern
        /// </summary>
        /// <param name="usage">How the parameter is used in the command</param>
        /// <param name="type">The system type of the parameter expected</param>
        /// <param name="cacheTypes">How this parameter can be found (is it data, is it a live entity)</param>
        /// <param name="optional">Is this parameter optional</param>
        public CommandParameterAttribute(CommandUsage usage, Type[] types, CacheReferenceType[] cacheTypes, bool optional, bool requiresPreviousParameter = false)
        {
            Usage = usage;
            ParameterTypes = types;
            CacheTypes = cacheTypes;
            RegExPattern = string.Empty;
            Optional = optional;
            RequiresPreviousParameter = requiresPreviousParameter;
        }

        /// <summary>
        /// Does the keywords incoming match the regex pattern (unless it's blank, in which case it always matches)
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public bool MatchesPattern(string inputString)
        {
            return string.IsNullOrWhiteSpace(RegExPattern) || Regex.IsMatch(inputString, RegExPattern, RegexOptions.IgnorePatternWhitespace);
        }
    }
}
