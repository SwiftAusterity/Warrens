using NetMud.DataStructure.Administrative;
using System;

namespace NetMud.Commands.Attributes
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
        /// Does the target require the zone be owned/allied to the Actor
        /// </summary>
        public bool RequiresTargetOwnership { get; private set; }

        /// <summary>
        /// Create a new permission attribute
        /// </summary>
        /// <param name="minimumRankAllowed">Minimum staff rank a player must be before they can "see" and use this command</param>
        /// <param name="requiresTargetOwnership">Does the target require the zone be owned/allied to the Actor</param>
        public CommandPermissionAttribute(StaffRank minimumRankAllowed, bool requiresTargetOwnership = false)
        {
            MinimumRank = minimumRankAllowed;
        }
    }
}
