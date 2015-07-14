using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=true)]
    public class CommandParameterAttribute : Attribute
    {
        public CommandUsage Usage { get; private set; }
        public Type ParameterType { get; private set; }
        public CacheReferenceType[] CacheTypes { get; private set; }

        public CommandParameterAttribute(CommandUsage usage, Type type, CacheReferenceType[] cacheTypes)
        {
            Usage = usage;
            ParameterType = type;
            CacheTypes = cacheTypes;
        }
    }

    public enum CommandUsage
    {
        Subject,
        Target,
        Container
    }

    public enum CacheReferenceType
    {
        Entity,
        Reference,
        Code,
        Help //hacky add for help specifically
    }
}
