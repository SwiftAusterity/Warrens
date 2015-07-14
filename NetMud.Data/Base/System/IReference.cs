using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.Data.Base.System
{
    public interface IReference : IComparable, IEquatable<IReference>
    {
        long ID { get; set; }
        DateTime Created { get; set; }
        DateTime LastRevised { get; set; }
        string Name { get; set; }

        void Fill (DataRow dr);
    }
}
