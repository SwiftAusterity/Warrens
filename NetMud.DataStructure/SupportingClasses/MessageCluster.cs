using System.Collections.Generic;

namespace NetMud.DataStructure.SupportingClasses
{
    public class MessageCluster
    {
        public string Actor { get; set; }
        public string Subject { get; set; }
        public string Target { get; set; }
        public string Origin { get; set; }
        public string Destination { get; set; }
        public Dictionary<int, string> Surrounding { get; set; }

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
