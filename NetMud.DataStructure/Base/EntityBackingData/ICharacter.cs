using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.SupportingClasses;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface ICharacter : IEntityBackingData, IGender
    {
        string SurName { get; set; }
        string AccountHandle { get; set; }
        IAccount Account { get; }
        StaffRank GamePermissionsRank { get; set; }

        string LastKnownLocation { get; set; }
        string LastKnownLocationType { get; set; }
        string FullName();
    }
}
