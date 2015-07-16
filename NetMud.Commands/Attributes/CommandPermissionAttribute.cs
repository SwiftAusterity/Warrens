using System;

namespace NutMud.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class CommandPermissionAttribute : Attribute
    {
        public StaffRank MinimumRank { get; private set; }

        public CommandPermissionAttribute(StaffRank minimumRankAllowed)
        {
            MinimumRank = minimumRankAllowed;
        }
    }

    public enum StaffRank
    {
        Player,
        Guest,
        Builder,
        Admin
    }
}
