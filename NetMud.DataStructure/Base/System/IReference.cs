using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface IReference : IData, IHelpful, IComparable, IEquatable<IReference>
    {
        string Name { get; set; }

    }
}
