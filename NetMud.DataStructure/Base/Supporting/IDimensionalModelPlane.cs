using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NetMud.DataStructure.Base.Supporting
{
    /// <summary>
    /// A single 11x11 grid of the 11 planes that compose the dimensional model
    /// </summary>
    public interface IDimensionalModelPlane
    {
        /// <summary>
        /// The collection of 121 nodes in the plane
        /// </summary>
        HashSet<IDimensionalModelNode> ModelNodes { get; set; }

        /// <summary>
        /// The name of this plane (for a sword it might be 'Blade' or 'Hilt')
        /// </summary>
        string TagName { get; set; }

        /// <summary>
        /// All nodes in a plane are of the same YAxis so bubble it up here so we have access
        /// </summary>
        short YAxis { get; set; }

        /// <summary>
        /// Gets a node based on the X and Y axis
        /// </summary>
        /// <param name="xAxis">the X-Axis of the node to get</param>
        /// <returns>the node</returns>
        IDimensionalModelNode GetNode(short xAxis, short zAxis);
    }
}
