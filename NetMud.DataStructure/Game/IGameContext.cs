using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.DataStructure.Game
{
    public interface IGameContext
    {
        HashSet<IPlayer> Players { get; set; }

        DateTime Created { get; set; }

        DateTime Ended { get; set; }

        IPlayer Winner { get; set; }

        ITurn CurrentTurn { get; set; }

        HashSet<ITurn> PriorTurns { get; set; }

        string State { get; set; }
    }
}
