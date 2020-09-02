using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Game
{
    public interface IMatchHistory
    {
        DateTime Started { get; set; }
        DateTime Ended { get; set; }
        IGame Game { get; set; }
        HashSet<IPlayer> Opponents { get; set; }
        string Results { get; set; }
        bool Winner { get; set; }
    }
}
