using NetMud.DataStructure.Behaviors.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Behaviors.Existential
{
    /// <summary>
    /// Var collection for IExist
    /// </summary>
    public interface IHasPositioning
    {
        /// <summary>
        /// x,y,z position in the world
        /// </summary>
        Tuple<long, long, long> Position { get; }

        /// <summary>
        /// What direction is this facing, 0/360 = north, 0-180 incline
        /// </summary>
        Tuple<int, int> Facing { get; }

        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IContains InsideOf { get; set; }
    }
}
