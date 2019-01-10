using System;

namespace NetMud.Commands.Attributes
{
    /// <summary>
    /// Dictates the range at which a command can be used
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false)]
    public class CommandRangeAttribute : Attribute
    {
        /// <summary>
        /// The type of range we're looking at
        /// </summary>
        public CommandRangeType Type { get; private set; }

        /// <summary>
        /// The maximum range a command can target from
        /// </summary>
        public int Value { get; private set; }

        /// <summary>
        /// Loads a new range attribute
        /// </summary>
        /// <param name="type">Range type</param>
        /// <param name="value">The maximum range a command can target from</param>
        public CommandRangeAttribute(CommandRangeType type, int value)
        {
            Type = type;
            Value = value;
        }
    }

    /// <summary>
    /// The type of range we're looking at for command execution
    /// </summary>
    public enum CommandRangeType
    {
        Global,
        Regional,
        Local,
        Touch,
        Self
    }
}
