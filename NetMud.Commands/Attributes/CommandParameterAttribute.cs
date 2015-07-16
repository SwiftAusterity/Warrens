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
