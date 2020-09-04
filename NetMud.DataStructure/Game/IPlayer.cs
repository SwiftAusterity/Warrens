using System.Collections.Generic;

namespace NetMud.DataStructure.Game
{
    public interface IPlayer
    {
        string System { get; set; }
        string Name { get; set; }

        HashSet<IMatchHistory> Matches { get; set; }
    }
}
