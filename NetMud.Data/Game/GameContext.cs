using NetMud.DataStructure.Game;
using NetMud.DataStructure.Player;
using System;
using System.Collections.Generic;

namespace NetMud.Data.Game
{
    [Serializable]
    public class GameContext : IGameContext
    {
        public HashSet<IPlayer> Players { get; set; }

        public DateTime Created { get; set; }

        public DateTime Ended { get; set; }

        public IPlayer Winner { get; set; }

        public ITurn CurrentTurn { get; set; }

        public HashSet<ITurn> PriorTurns { get; set; }

        public string State { get; set; }

        public GameContext()
        {
            Players = new HashSet<IPlayer>();
            PriorTurns = new HashSet<ITurn>();
            CurrentTurn = new Turn();
            Created = DateTime.Now;
        }
    }
}
