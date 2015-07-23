using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface ICharacter : IEntityBackingData, IGender
    {
        string SurName { get; set; }
        string AccountHandle { get; set; }
        IAccount Account { get; }

        string LastKnownLocation { get; set; }
        string LastKnownLocationType { get; set; }
        string FullName();
    }
}
