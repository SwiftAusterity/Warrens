using System;

namespace NutMud.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class CommandParameterAttribute : Attribute
    {
        public CommandUsage Usage { get; private set; }
        public Type ParameterType { get; private set; }
        public CacheReferenceType[] CacheTypes { get; private set; }
        public bool Optional { get; private set; }

        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes, bool optional)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
            Optional = optional;
        }
    }

    public enum CommandUsage
    {
        Subject,
        Target,
        Supporting,
        Location
    }

    public enum CacheReferenceType
    {
        Entity,
        Reference,
        Code,
        Container,
        Help //hacky add for help specifically
    }
}
