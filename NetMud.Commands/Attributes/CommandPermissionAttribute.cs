using NetMud.DataStructure.SupportingClasses;
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
}
