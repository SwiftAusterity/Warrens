using NetMud.DataStructure.Base.Place;
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
        /// x,y,z position in the worlds
        /// </summary>
        IGlobalPosition Position { get; }

        /// <summary>
        /// What direction is this facing, 0/360 = north, 0-180 incline
        /// </summary>
        Tuple<int, int> Facing { get; }

        /// <summary>
        /// Current location this entity is in
        /// </summary>
        IContains InsideOf { get; set; }
    }

    /// <summary>
    /// Coords + world designator
    /// </summary>
    public interface IGlobalPosition
    {
        /// <summary>
        /// x,y,z position in the world
        /// </summary>
        Tuple<long, long, long> Coordinates { get; set; }

        /// <summary>
        /// The world this is in
        /// </summary>
        IWorld World { get; set;  }
    }
}
