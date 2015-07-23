using System;
using System.Data;

namespace NetMud.DataStructure.Base.System
{
    public interface IData : IComparable<IData>, IEquatable<IData>
    {
        long ID { get; set; }
        DateTime Created { get; set; }
        DateTime LastRevised { get; set; }
        string Name { get; set; }

        void Fill(DataRow dr);
        IData Create();
        bool Remove();
        bool Save();
    }
}
