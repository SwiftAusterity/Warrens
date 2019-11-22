using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using System;

namespace NetMud.DataStructure.Graces
{
    public interface IReply
    {
        IGrapevineUser Author { get; set; }

        MarkdownString Body { get; set; }

        DateTime Birthdate { get; set; }
    }
}
