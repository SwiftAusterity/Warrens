using NetMud.DataStructure.Base.Supporting;
using NetMud.DataStructure.Base.System;
using NetMud.DataStructure.Behaviors.Rendering;
using NetMud.DataStructure.Behaviors.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Place
{
    /// <summary>
    /// Contains live handling for things in the world
    /// </summary>
    public interface IChunk : ILocation, ISpawnAsSingleton
    {
        /// <summary>
        /// Upper x,y,z
        /// </summary>
        Tuple<long, long, long> UpperBounds { get; set; }

        /// <summary>
        /// Lower x,y,z
        /// </summary>
        Tuple<long, long, long> LowerBounds { get; set; }

        /// <summary>
        /// What world do these belong to
        /// </summary>
        IWorld World { get; set; }
    }
}
