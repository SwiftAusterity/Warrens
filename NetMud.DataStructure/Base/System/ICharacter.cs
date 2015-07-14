using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface ICharacter : IReference
    {
        string SurName { get; set; }
        string GivenName { get; set; }
        string AccountHandle { get; set; }
        IAccount Account { get; }
    }
}
