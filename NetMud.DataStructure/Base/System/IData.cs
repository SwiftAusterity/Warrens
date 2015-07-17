using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
