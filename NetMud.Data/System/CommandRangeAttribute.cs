using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.System
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class CommandRangeAttribute : Attribute
    {
        public CommandRangeType Type { get; private set; }
        public int Value { get; private set; }

        public CommandRangeAttribute(CommandRangeType type, int value)
        {
            Type = type;
            Value = value;
        }
    }

    public enum CommandRangeType
    {
        Global,
        Regional,
        Local,
        Touch,
        Self
    }
}
