using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;

namespace NetMud.DataStructure.Base.EntityBackingData
{
    public interface INonPlayerCharacter : IEntityBackingData, IGender
    {
        string SurName { get; set; }
        string FullName();
    }
}
