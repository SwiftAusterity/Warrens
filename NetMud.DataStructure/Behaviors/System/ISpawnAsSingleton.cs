using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.System
{
    /// <summary>
    /// Add this if there should only ever be one of each in the world for each backingData entry. Players, rooms, exits/paths, channels? not sure what else yet
    /// </summary>
    public interface ISpawnAsSingleton
    {
        void GetFromWorldOrSpawn();
    }
}
