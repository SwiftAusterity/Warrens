using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.System
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
