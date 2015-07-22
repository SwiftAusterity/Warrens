using System;
using System.Text.RegularExpressions;

namespace NutMud.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class CommandParameterAttribute : Attribute
    {
        public CommandUsage Usage { get; private set; }
        public Type ParameterType { get; private set; }
        public CacheReferenceType[] CacheTypes { get; private set; }
        public string RegExPattern { get; private set; }
        public bool Optional { get; private set; }

        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, string matchingPattern, bool optional)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
            RegExPattern = matchingPattern;
            Optional = optional;
        }

        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, bool optional)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
            RegExPattern = string.Empty;
            Optional = optional;
        }

        public bool MatchesPattern(string inputString)
        {
            return string.IsNullOrWhiteSpace(RegExPattern) || Regex.IsMatch(inputString, RegExPattern);
        }
    }

    public enum CommandUsage : short
    {
        Subject = 0,
        Target = 1,
        Supporting = 2
    }

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
