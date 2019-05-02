using NetMud.DataStructure.Player;
using System;

namespace NetMud.DataStructure.Game
{
    public interface ITurn
    {
        int Number { get; set; }

        IPlayer Taker { get; set; }

        DateTime Start { get; set; }

        DateTime TrueEnd { get; set; }

        DateTime ScheduledEnd { get; set; }

        string Move { get; set; }
    }
}
