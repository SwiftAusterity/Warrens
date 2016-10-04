using System;

namespace NutMud.Commands.Attributes
{
    /// <summary>
    /// Force suppression of command name in help and commands listings
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class CommandSuppressName : Attribute
    {
        /// <summary>
        /// Creates a new keyword attribute
        /// </summary>
        public CommandSuppressName()
        {
        }
    }
}
