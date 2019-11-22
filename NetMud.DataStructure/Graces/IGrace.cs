using NetMud.DataStructure.Architectural;
using NetMud.DataStructure.Gossip;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Graces
{
    public interface IGrace
    {
        IGrapevineUser Author { get; set; }

        MarkdownString Body { get; set; }

        DateTime Birthdate { get; set; }

        HashSet<IReply> Replies { get; set; }

        string Reply(MarkdownString body);
    }
}
