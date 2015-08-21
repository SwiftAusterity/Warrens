using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.System
{
    public interface IHelp : IReferenceData
    {
        string HelpText { get; set; }
    }
}
