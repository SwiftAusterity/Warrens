using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Game
{
    public interface IPlayer
    {
        string System { get; set; }
        string Name { get; set; }

        HashSet<IMatchHistory> Matches { get; set; }
    }
}
