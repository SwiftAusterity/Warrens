using System;

namespace NetMud.DataStructure.Base.System
{
    public interface IEntityBackingData : IData
    {
        Type EntityClass { get; }
    }
}
