using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Game
{
    public interface IMatch
    {
        DateTime Started { get; set; }
        IDictionary<short, IPlayer> Participants { get; set; }
        short CurrentPlayer { get; set; }
        int CurrentTurn { get; set; }

    }
}
