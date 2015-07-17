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

        public MessageCluster()
        {
            Surrounding = new Dictionary<int, string>();
        }

        public MessageCluster(string actor, string subject, string target, string origin, string destination)
        {
            Actor = actor;
            Subject = subject;
            Target = target;
            Origin = origin;
            Destination = destination;

            Surrounding = new Dictionary<int, string>();
        }
    }
}
