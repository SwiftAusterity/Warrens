using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.SupportingClasses
{
    public class MessageCluster
    {
        string Actor { get; set; }
        string Subject { get; set; }
        string Target { get; set; }
        string Origin { get; set; }
        string Destination { get; set; }
        Dictionary<int, string> Surrounding { get; set; }
    }
}
