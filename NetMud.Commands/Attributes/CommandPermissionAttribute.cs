using NetMud.DataStructure.SupportingClasses;
using System;

namespace NutMud.Commands.Attributes
{
    /// <summary>
    /// Staff rank permissions for executing commands
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPermissionAttribute : Attribute
    {
        /// <summary>
        /// Minimum staff rank a player must be before they can "see" and use this command
        /// </summary>
        public StaffRank MinimumRank { get; private set; }

        /// <summary>
        /// Create a new permission attribute
        /// </summary>
        /// <param name="minimumRankAllowed">Minimum staff rank a player must be before they can "see" and use this command</param>
        public CommandPermissionAttribute(StaffRank minimumRankAllowed)
        {
            MinimumRank = minimumRankAllowed;
        }
    }
}
