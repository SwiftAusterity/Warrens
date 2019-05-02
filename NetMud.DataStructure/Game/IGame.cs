using NetMud.DataStructure.Architectural;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Game
{
    public interface IGame
    {
        string Name { get; set; }

        ValueRange<short> NumberOfPlayers { get; set; }

        string Description { get; set; }

        int AverageDuration { get; set; }

        bool HighScoreboard { get; set; }

        bool PublicReplay { get; set; }
    }
}
